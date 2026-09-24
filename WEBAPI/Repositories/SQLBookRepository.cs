using Microsoft.EntityFrameworkCore;
using WebAPI.Data;
using WebAPI.Models.Domain;
using WebAPI.Models.DTO;

namespace WebAPI.Repositories
{
    public class SQLBookRepository : IBookRepository
    {
        private readonly AppDbContext _dbContext;

        public SQLBookRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public List<BookWithAuthorAndPublisherDTO> GetAllBooks()
        {
            var allBooks = _dbContext.Books.Select(book => new BookWithAuthorAndPublisherDTO()
            {
                Id = book.Id,
                Title = book.Title,
                Description = book.Description,
                IsRead = book.IsRead,
                DateRead = book.IsRead ? book.DateRead.Value : null,
                Rate = book.IsRead ? book.Rate.Value : null,
                Genre = book.Genre,
                CoverUrl = book.CoverUrl,
                PublisherName = book.Publisher.Name,
                AuthorNames = book.Book_Authors.Select(n => n.Author.FullName).ToList()
            }).ToList();

            return allBooks;
        }

        public BookWithAuthorAndPublisherDTO? GetBookById(int id)
        {
            var bookWithDomain = _dbContext.Books.Where(n => n.Id == id);

            var bookWithIdDTO = bookWithDomain.Select(book => new BookWithAuthorAndPublisherDTO()
            {
                Id = book.Id,
                Title = book.Title,
                Description = book.Description,
                IsRead = book.IsRead,
                DateRead = book.DateRead,
                Rate = book.Rate,
                Genre = book.Genre,
                CoverUrl = book.CoverUrl,
                PublisherName = book.Publisher.Name,
                AuthorNames = book.Book_Authors.Select(n => n.Author.FullName).ToList()
            }).FirstOrDefault();

            return bookWithIdDTO;
        }

        public AddBookRequestDTO AddBook(AddBookRequestDTO addBookRequestDTO)
        {
            var bookDomainModel = new Book
            {
                Title = addBookRequestDTO.Title,
                Description = addBookRequestDTO.Description,
                IsRead = addBookRequestDTO.IsRead,
                DateRead = addBookRequestDTO.DateRead,
                Rate = addBookRequestDTO.Rate,
                Genre = addBookRequestDTO.Genre,
                CoverUrl = addBookRequestDTO.CoverUrl,
                DateAdded = addBookRequestDTO.DateAdded,
                PublisherID = addBookRequestDTO.PublisherID
            };

            _dbContext.Books.Add(bookDomainModel);
            _dbContext.SaveChanges();

            foreach (var id in addBookRequestDTO.AuthorIds)
            {
                var bookAuthor = new Book_Author()
                {
                    BookId = bookDomainModel.Id,
                    AuthorId = id
                };
                _dbContext.Books_Authors.Add(bookAuthor);
                _dbContext.SaveChanges();
            }

            return addBookRequestDTO;
        }

        public AddBookRequestDTO? UpdateBookById(int id, AddBookRequestDTO bookDTO)
        {
            var bookDomain = _dbContext.Books.FirstOrDefault(n => n.Id == id);
            if (bookDomain != null)
            {
                bookDomain.Title = bookDTO.Title;
                bookDomain.Description = bookDTO.Description;
                bookDomain.IsRead = bookDTO.IsRead;
                bookDomain.DateRead = bookDTO.DateRead;
                bookDomain.Rate = bookDTO.Rate;
                bookDomain.Genre = bookDTO.Genre;
                bookDomain.CoverUrl = bookDTO.CoverUrl;
                bookDomain.DateAdded = bookDTO.DateAdded;
                bookDomain.PublisherID = bookDTO.PublisherID;

                _dbContext.SaveChanges();

                var authorDomain = _dbContext.Books_Authors.Where(a => a.BookId == id).ToList();
                if (authorDomain != null && authorDomain.Count > 0)
                {
                    _dbContext.Books_Authors.RemoveRange(authorDomain);
                    _dbContext.SaveChanges();
                }

                foreach (var authorId in bookDTO.AuthorIds)
                {
                    var bookAuthor = new Book_Author()
                    {
                        BookId = id,
                        AuthorId = authorId
                    };
                    _dbContext.Books_Authors.Add(bookAuthor);
                    _dbContext.SaveChanges();
                }

                return bookDTO;
            }

            return null;
        }

        public Book? DeleteBookById(int id)
        {
            var bookDomain = _dbContext.Books.FirstOrDefault(n => n.Id == id);
            if (bookDomain != null)
            {
                var existingBookAuthors = _dbContext.Books_Authors.Where(x => x.BookId == id).ToList();
                if (existingBookAuthors != null && existingBookAuthors.Count > 0)
                {
                    _dbContext.Books_Authors.RemoveRange(existingBookAuthors);
                    _dbContext.SaveChanges();
                }

                _dbContext.Books.Remove(bookDomain);
                _dbContext.SaveChanges();
                return bookDomain;
            }
            return null;
        }
    }
}