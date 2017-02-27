using AutoMapper;
using DataWebDownload.Helpers;
using EntityFramework.Utilities;
using Heroic.AutoMapper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Console = Colorful.Console;

namespace DataWebDownload
{
    class Program
    {
        static HttpClient Client;
        private static string _apiEndPoint;
        private static double _runTime;
        private static int _interval;
        private static DateTime _startDate;
        private static string _discriminator;

        private static void Main()
        {
            HeroicAutoMapperConfigurator.LoadMapsFromCallerAndReferencedAssemblies();
            var goAgain = true;
            while (goAgain)
            {
                CollectAnswers().Wait();
                RunAsync().Wait();
                goAgain = false;
                Console.WriteLine("Finished", Color.Blue);
                Console.WriteLine();
                Console.Write("Continue (Y/N)?: ");
                var answer = Console.ReadLine();
                if (answer.ToLower() == "y") goAgain = true;

            }

            Console.Write("Finished");

        }

        static async Task CollectAnswers()
        {
            Client = new HttpClient();
            string line;
            var isValidUrl = false;
            while (!isValidUrl)
            {
                try
                {
                    Console.Write("Enter URL: ");
                    line = Console.ReadLine();
                    Client.BaseAddress = new Uri(line);
                    Client.DefaultRequestHeaders.Accept.Clear();
                    Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    _apiEndPoint = line;
                    isValidUrl = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Invalid URL, please retry");
                    isValidUrl = false;
                }

            }
            line = "";
            while (string.IsNullOrWhiteSpace(line))
            {
                Console.Write("Enter discriminator: ");
                line = Console.ReadLine();
                _discriminator = line;
                if (string.IsNullOrWhiteSpace(line)) Console.WriteLine("Invalid discriminator, please retry");
            }

            Console.Write("Enter start date/time yyyy-MM-dd HH:mm: ");
            line = Console.ReadLine();
            while (!DateTime.TryParseExact(line, "yyyy-MM-dd HH:mm", null, DateTimeStyles.AssumeLocal, out _startDate))
            {
                Console.WriteLine("Invalid date, please retry");
                Console.Write("Enter start date/time yyyy-MM-dd HH:mm: ");
                line = Console.ReadLine();
            }
            Console.Write("Enter duration in hours: ");
            line = Console.ReadLine();
            while (!double.TryParse(line, out _runTime))
            {
                Console.WriteLine("Invalid duration, please retry");
                Console.Write("Enter duration in hours: ");
                line = Console.ReadLine();
            }
            Console.Write("Enter interval in minutes: ");
            line = Console.ReadLine();
            while (!int.TryParse(line, out _interval))
            {
                Console.WriteLine("Invalid interval, please retry");
                Console.Write("Enter interval in minutes: ");
                line = Console.ReadLine();
            }

        }

        static async Task RunAsync()
        {
            var errors = new List<ErrorMessage>();
            try
            {
                var endDate = _startDate.AddHours(_runTime);

                Console.WriteLine($"Retrieving dates: {_startDate} - {endDate}", Color.LightSkyBlue);
                bool result;
                do
                {
                    if (_startDate >= endDate) { result = false; continue; }

                    Console.WriteLine($"Retrieving {_startDate} to {_startDate.AddMinutes(_interval)} from", Color.Gold);
                    Console.WriteLine($"{_apiEndPoint}/{_startDate.EpochTime()}/{_startDate.AddMinutes(_interval).EpochTime()} ", Color.GreenYellow);
                    var statusResult = await GetRecordsAsync($"{_apiEndPoint}/{_startDate.EpochTime()}/{_startDate.AddMinutes(_interval).EpochTime()}");
                    if (!statusResult.Success)
                    {

                        statusResult.ErrorMessage.StartTime = _startDate.ToString("G");
                        statusResult.ErrorMessage.EndTime = _startDate.AddMinutes(_interval).ToString("G");

                        Console.WriteLine($"Failed {statusResult.ErrorMessage.StartTime}-{statusResult.ErrorMessage.EndTime}");
                        errors.Add(statusResult.ErrorMessage);
                    }

                    var list = Mapper.Map<List<Person>>(statusResult.Persons);

                    result = await SaveRecords(list);

                    if (!result)
                    {
                        var error = new ErrorMessage() { Message = "Failed to save data", StartTime = _startDate.ToString("G"), EndTime = _startDate.AddMinutes(_interval).ToString("G") };
                        errors.Add(error);
                    }

                    _startDate = _startDate.AddMinutes(_interval);

                } while (result);
                if (errors.Count > 0)
                {
                    Console.WriteLine($"The following failed");
                    foreach (var message in errors)
                    {
                        System.Console.WriteLine($"{message.StartTime}  {message.EndTime} Message: {message.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}", Color.Red);
                return;
            }

        }

        static async Task<OperationResult> GetRecordsAsync(string path)
        {
            var result = new OperationResult();
            List<PersonViewModel> records = null;

            try
            {
                var response = await Client.GetAsync(path);
                if (response.IsSuccessStatusCode)
                {
                    result.Persons = await response.Content.ReadAsAsync<List<PersonViewModel>>();
                }
                else
                {
                    var message = new ErrorMessage() { Message = $"Failed {response.StatusCode}" };
                    Console.WriteLine($"{message.Message}");
                    result.ErrorMessage = message;
                }
                result.Success = response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
                Console.WriteLine($"Error occurred: {ex.Message}", Color.Red);
            }
            return result;
        }

        static async Task<bool> SaveRecords(List<Person> list)
        {
            if (list == null) return true;
            try
            {
                using (var db = DataContext.Create())
                {
                    if (list.Count == 0) return true;
                    foreach (var person in list)
                    {
                        person.Discriminator = _discriminator;
                    }
                    EFBatchOperation.For(db, db.Persons).InsertAll(list);
                    await db.SaveChangesAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message, Color.Red);
                return false;
            }
        }
    }

    internal class ErrorMessage
    {
        public string Message { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }

    internal class OperationResult
    {
        public bool Success { get; set; }
        public List<PersonViewModel> Persons { get; set; }
        public string Message { get; set; }
        public ErrorMessage ErrorMessage { get; set; }
    }
}

//https://www.splcenter.org/admin/splc-entityform-submissions/vEHCjGUdJoiqftMUsS1RXGUymuog4rlZyJnawUqTHYzUUp3tGP/tell_donald_trump_to_reject_hate

//apiEndPoint =
//    $"https://www.splcenter.org/admin/splc-entityform-submissions/vEHCjGUdJoiqftMUsS1RXGUymuog4rlZyJnawUqTHYzUUp3tGP/tell_donald_trump_to_reject_hate/{startDate}/{endDate}";



//apiEndPoint =
//    "https://www.splcenter.org/admin/splc-entityform-submissions/vEHCjGUdJoiqftMUsS1RXGUymuog4rlZyJnawUqTHYzUUp3tGP/tell_donald_trump_to_reject_hate/1478995149/1478995149";

//apiEndPoint =
//    $"https://www.splcenter.org/admin/splc-entityform-submissions/vEHCjGUdJoiqftMUsS1RXGUymuog4rlZyJnawUqTHYzUUp3tGP/tell_donald_trump_to_reject_hate/{startDate.EpochTime()}/{startDate.AddHours(1).EpochTime()}";
//apiEndPoint =
//    $"http://live-splc.pantheon.io/admin/splc-entityform-submissions/vEHCjGUdJoiqftMUsS1RXGUymuog4rlZyJnawUqTHYzUUp3tGP/say_no_to_stephen_bannon/{startDate.EpochTime()}/{startDate.AddMinutes(10).EpochTime()}";
