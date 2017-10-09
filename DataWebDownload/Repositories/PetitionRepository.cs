using DataWebDownload.Models;
using System.Collections.Generic;
using System.Linq;
using DataWebDownload.Persitence;

namespace DataWebDownload.Repositories
{
    public class PetitionRepository
    {
        private readonly DataContext _context;

        public PetitionRepository(DataContext context)
        {
            _context = context;
        }


        public void Add(Petition petition)
        {
            _context.Petitions.Add(petition);
        }

        public void AddRange(List<Petition> list)
        {
            _context.Petitions.AddRange(list);
        }

        public IEnumerable<Petition> GetLast50()
        {
            return _context.Petitions.OrderByDescending(p => p.Id).Skip(0).Take(50);
        }
    }
}