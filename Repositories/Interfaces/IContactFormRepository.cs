namespace ContactApplication.Repositories.Interfaces
{
    public interface IContactFormRepository
    {
        void InsertContactForm(string firstName, string lastName, string email, string phone, string zip, string city, string state, string comments, DateTime createdAt);

    }
}
