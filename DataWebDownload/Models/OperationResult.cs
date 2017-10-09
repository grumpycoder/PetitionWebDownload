using System.Collections.Generic;
using DataWebDownload.ViewModels;

namespace DataWebDownload.Models
{
    internal class OperationResult
    {
        public bool Success { get; set; }
        public List<TrumpPetitionViewModel> Petitions { get; set; }
        public string Message { get; set; }
        public ErrorMessage ErrorMessage { get; set; }
    }
}