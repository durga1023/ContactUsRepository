using ContactApplication.Repositories;
using ContactApplication.Repositories.DataObjects;
using ContactApplication.Repositories.Interfaces;

namespace ContactApplication.Repositories
{
    public class ContactFormRepository : IContactFormRepository
    {
        private readonly AppDbContext _context;

        public ContactFormRepository(AppDbContext context)
        {
            _context = context;
        }

        public void InsertContactForm(string firstName, string lastName, string email, string phone, string zip, string city, string state, string comments, DateTime createdAt)
        {
            var contact = new Contact
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Phone = phone,
                Zip = zip,
                City = city,
                State = state,
                Comments = comments,
                CreatedAt= createdAt
            };

            _context.Contact.Add(contact);
            _context.SaveChanges();
        }
    }
}
