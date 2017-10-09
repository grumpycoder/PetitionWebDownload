using AutoMapper;
using DataWebDownload.Helpers;
using DataWebDownload.Models;
using DataWebDownload.Persitence;
using DataWebDownload.Repositories;
using DataWebDownload.ViewModels;
using Heroic.AutoMapper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Net;
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
        private static DateTime _endDate;
        public static UnitOfWork _uow;

        private static void Main()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            HeroicAutoMapperConfigurator.LoadMapsFromCallerAndReferencedAssemblies();
            _uow = new UnitOfWork(new DataContext());

            var goAgain = true;
            while (goAgain)
            {
                CollectAnswers();
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

        private static async Task<IEnumerable<TrumpPetitionViewModel>> GetRecords(string url)
        {
            //var context = new DataContext();

            System.Console.WriteLine($"Getting records: { url }");
            IEnumerable<TrumpPetitionViewModel> records = null;

            try
            {

                var response = await Client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    records = await response.Content.ReadAsAsync<List<TrumpPetitionViewModel>>();
                }
                else
                {
                    var message = new ErrorMessage() { Message = $"Failed {response.StatusCode}" };
                    Console.WriteLine($"{message.Message}");
                }
            }
            catch
            {
                Console.WriteLine("Error");
            }
            var list = Mapper.Map<List<Petition>>(records);

            //foreach (var petition in list)
            //{
            //    context.Petitions.Add(petition);
            //    //_uow.Petitions.Add(petition);
            //}
            //context.SaveChanges();
            _uow.Petitions.AddRange(list);
            _uow.Complete();

            return records;


        }

        private static void CollectAnswers()
        {
            Client = new HttpClient();
            string line;
            var isValidUrl = false;
            // Check for valid url
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
            Console.Write("Enter duration in minutes: ");
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
                var startTime = _startDate;
                _endDate = _startDate.AddMinutes(_runTime);
                var localEndTime = _startDate.AddMinutes(_interval);

                Console.WriteLine($"Retrieving dates: {startTime} - {localEndTime}", Color.LightSkyBlue);

                var result = true;
                do
                {
                    if (startTime >= _endDate) { result = false; continue; }

                    Console.WriteLine($"Retrieving {startTime} to { localEndTime }", Color.Gold);
                    var statusResult = await GetRecordsAsync($"{_apiEndPoint}/{startTime.EpochTime()}/{localEndTime.EpochTime()}");

                    if (!statusResult.Success)
                    {
                        statusResult.ErrorMessage.StartTime = startTime.ToString("G");
                        statusResult.ErrorMessage.EndTime = localEndTime.ToString("G");

                        Console.WriteLine($"Failed {statusResult.ErrorMessage.StartTime}-{statusResult.ErrorMessage.EndTime}");
                        errors.Add(statusResult.ErrorMessage);
                    }

                    var petitions = Mapper.Map<List<Petition>>(statusResult.Petitions);

                    foreach (var petition in petitions)
                    {
                        petition.Discriminator = _discriminator;
                    }

                    _uow.Petitions.AddRange(petitions);

                    _uow.Complete();

                    startTime = startTime.AddMinutes(_interval);
                    localEndTime = localEndTime.AddMinutes(_interval);

                } while (result);
                if (errors.Count > 0)
                {
                    Console.WriteLine($"The following failed");
                    foreach (var message in errors)
                    {
                        Console.WriteLine($"{message.StartTime}  {message.EndTime} Message: {message.Message}", Color.Red);
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
            System.Console.WriteLine($"Getting records: { path }");
            var result = new OperationResult();

            try
            {
                var response = await Client.GetAsync(path);
                if (response.IsSuccessStatusCode)
                {
                    result.Petitions = await response.Content.ReadAsAsync<List<TrumpPetitionViewModel>>();
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

    }
}
