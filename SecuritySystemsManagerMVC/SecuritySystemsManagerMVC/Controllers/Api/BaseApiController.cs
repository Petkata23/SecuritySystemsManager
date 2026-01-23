using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Services.Contracts;
using System.Reflection;

namespace SecuritySystemsManagerMVC.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public abstract class BaseApiController<TModel, TService> : ControllerBase
        where TModel : BaseDto
        where TService : class
    {
        protected readonly TService _service;
        protected const int DefaultPageSize = 10;
        protected const int DefaultPageNumber = 1;
        protected const int MaxPageSize = 100;

        protected BaseApiController(TService service)
        {
            _service = service;
        }

        // Helper methods to call service methods via reflection
        protected async Task<IEnumerable<TModel>> GetWithPaginationAsync(int pageSize, int pageNumber)
        {
            var method = _service.GetType().GetMethod("GetWithPaginationAsync");
            if (method == null)
                throw new InvalidOperationException("Service does not implement GetWithPaginationAsync");
            
            var result = method.Invoke(_service, new object[] { pageSize, pageNumber });
            return await (Task<IEnumerable<TModel>>)result;
        }

        protected async Task<IEnumerable<TModel>> GetAllAsync()
        {
            var method = _service.GetType().GetMethod("GetAllAsync");
            if (method == null)
                throw new InvalidOperationException("Service does not implement GetAllAsync");
            
            var result = method.Invoke(_service, null);
            return await (Task<IEnumerable<TModel>>)result;
        }

        protected async Task<TModel> GetByIdIfExistsAsync(int id)
        {
            var method = _service.GetType().GetMethod("GetByIdIfExistsAsync");
            if (method == null)
                throw new InvalidOperationException("Service does not implement GetByIdIfExistsAsync");
            
            var result = method.Invoke(_service, new object[] { id });
            return await (Task<TModel>)result;
        }

        protected async Task SaveAsync(TModel model)
        {
            var method = _service.GetType().GetMethod("SaveAsync");
            if (method == null)
                throw new InvalidOperationException("Service does not implement SaveAsync");
            
            var result = method.Invoke(_service, new object[] { model });
            await (Task)result;
        }

        protected async Task<bool> ExistsByIdAsync(int id)
        {
            var method = _service.GetType().GetMethod("ExistsByIdAsync");
            if (method == null)
                throw new InvalidOperationException("Service does not implement ExistsByIdAsync");
            
            var result = method.Invoke(_service, new object[] { id });
            return await (Task<bool>)result;
        }

        protected async Task DeleteAsync(int id)
        {
            var method = _service.GetType().GetMethod("DeleteAsync");
            if (method == null)
                throw new InvalidOperationException("Service does not implement DeleteAsync");
            
            var result = method.Invoke(_service, new object[] { id });
            await (Task)result;
        }

        [HttpGet]
        public virtual async Task<IActionResult> GetAll([FromQuery] int pageSize = DefaultPageSize, [FromQuery] int pageNumber = DefaultPageNumber)
        {
            try
            {
                if (pageSize <= 0 || pageSize > MaxPageSize || pageNumber <= 0)
                {
                    return BadRequest(new { message = "Invalid pagination parameters" });
                }

                var models = await GetWithPaginationAsync(pageSize, pageNumber);
                var totalRecords = await GetAllAsync();
                var totalCount = totalRecords.Count();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                return Ok(new
                {
                    data = models,
                    pagination = new
                    {
                        pageNumber = pageNumber,
                        pageSize = pageSize,
                        totalCount = totalCount,
                        totalPages = totalPages
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving data", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public virtual async Task<IActionResult> GetById(int id)
        {
            try
            {
                var model = await GetByIdIfExistsAsync(id);
                if (model == null)
                {
                    return NotFound(new { message = "Resource not found" });
                }
                return Ok(model);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving data", error = ex.Message });
            }
        }

        [HttpPost]
        public virtual async Task<IActionResult> Create([FromBody] TModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                await SaveAsync(model);
                return CreatedAtAction(nameof(GetById), new { id = model.Id }, model);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the resource", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public virtual async Task<IActionResult> Update(int id, [FromBody] TModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (!await ExistsByIdAsync(id))
                {
                    return NotFound(new { message = "Resource not found" });
                }

                model.Id = id;
                await SaveAsync(model);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the resource", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public virtual async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (!await ExistsByIdAsync(id))
                {
                    return NotFound(new { message = "Resource not found" });
                }

                await DeleteAsync(id);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the resource", error = ex.Message });
            }
        }
    }
}
