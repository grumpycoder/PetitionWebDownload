using DataWebDownload.Persitence;
using System;

namespace DataWebDownload.Repositories
{
    public class UnitOfWork
    {
        private DataContext _context;
        public PetitionRepository Petitions { get; set; }

        public UnitOfWork(DataContext context)
        {
            _context = context;
            Petitions = new PetitionRepository(_context);
        }

        public void Complete()
        {
            try
            {
                var i = _context.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error saving: {e.InnerException.Message}");
            }
        }
    }
}