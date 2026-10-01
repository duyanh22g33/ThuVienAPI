using Microsoft.AspNetCore.Mvc;
using WebAPI.CustomActionFilters;
using WebAPI.Models.DTO;
using WebAPI.Repositories;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly IBookRepository _bookRepository;

        public BooksController(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository;
        }

        [HttpGet("get-all-books")]
        public IActionResult GetAll(
            [FromQuery] string? filterOn,
            [FromQuery] string? filterQuery,
            [FromQuery] string? sortBy,
            [FromQuery] bool isAscending = true,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 100)
        {
            var allBooks = _bookRepository.GetAllBooks(filterOn, filterQuery, sortBy, isAscending, pageNumber, pageSize);
            return Ok(allBooks);
        }

        [HttpGet]
        [Route("get-book-by-id/{id}")]
        public IActionResult GetBookById([FromRoute] int id)
        {
            var bookWithIdDTO = _bookRepository.GetBookById(id);
            if (bookWithIdDTO == null)
            {
                return NotFound(new { message = $"Không tìm thấy sách với Id = {id}" });
            }
            return Ok(bookWithIdDTO);
        }

        [HttpPost("add-book")]
        [ValidateModel]
        public IActionResult AddBook([FromBody] AddBookRequestDTO addBookRequestDTO)
        {
            if (!ValidateAddBook(addBookRequestDTO))
            {
                return BadRequest(ModelState);
            }

            if (!_bookRepository.PublisherExists(addBookRequestDTO.PublisherID))
            {
                return NotFound(new { message = $"Nhà xuất bản có Id = {addBookRequestDTO.PublisherID} không tồn tại!" });
            }

            foreach (var authorId in addBookRequestDTO.AuthorIds)
            {
                if (!_bookRepository.AuthorExists(authorId))
                {
                    return NotFound(new { message = $"Tác giả có Id = {authorId} không tồn tại!" });
                }
            }

            var bookAdd = _bookRepository.AddBook(addBookRequestDTO);
            return Ok(bookAdd);
        }

        [HttpPut("update-book-by-id/{id}")]
        [ValidateModel]
        public IActionResult UpdateBookById(int id, [FromBody] AddBookRequestDTO bookDTO)
        {
            if (!ValidateAddBook(bookDTO))
            {
                return BadRequest(ModelState);
            }

            if (!_bookRepository.PublisherExists(bookDTO.PublisherID))
            {
                return NotFound(new { message = $"Nhà xuất bản có Id = {bookDTO.PublisherID} không tồn tại!" });
            }

            foreach (var authorId in bookDTO.AuthorIds)
            {
                if (!_bookRepository.AuthorExists(authorId))
                {
                    return NotFound(new { message = $"Tác giả có Id = {authorId} không tồn tại!" });
                }
            }

            var updateBook = _bookRepository.UpdateBookById(id, bookDTO);
            if (updateBook == null)
            {
                return NotFound(new { message = $"Không tìm thấy sách có Id = {id} để cập nhật!" });
            }
            return Ok(updateBook);
        }

        [HttpDelete("delete-book-by-id/{id}")]
        public IActionResult DeleteBookById(int id)
        {
            var deleteBook = _bookRepository.DeleteBookById(id);
            if (deleteBook == null)
            {
                return NotFound(new { message = $"Không tìm thấy sách có Id = {id} để xóa!" });
            }
            return Ok(deleteBook);
        }

        #region Private methods
        private bool ValidateAddBook(AddBookRequestDTO addBookRequestDTO)
        {
            if (addBookRequestDTO == null)
            {
                ModelState.AddModelError(nameof(addBookRequestDTO), $"Please add book data");
                return false;
            }

            if (string.IsNullOrEmpty(addBookRequestDTO.Description))
            {
                ModelState.AddModelError(nameof(addBookRequestDTO.Description),
                    $"{nameof(addBookRequestDTO.Description)} cannot be null");
            }

            if (addBookRequestDTO.Rate < 0 || addBookRequestDTO.Rate > 5)
            {
                ModelState.AddModelError(nameof(addBookRequestDTO.Rate),
                    $"{nameof(addBookRequestDTO.Rate)} cannot be less than 0 and more than 5");
            }

            if (ModelState.ErrorCount > 0)
            {
                return false;
            }

            return true;
        }
        #endregion
    }
}