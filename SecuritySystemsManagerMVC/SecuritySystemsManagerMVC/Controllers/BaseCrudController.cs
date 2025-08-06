using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SecuritySystemsManager.Shared;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Extensions;
using SecuritySystemsManager.Shared.Repos.Contracts;
using SecuritySystemsManager.Shared.Services.Contracts;
using SecuritySystemsManagerMVC.ViewModels;

namespace SecuritySystemsManagerMVC.Controllers
{
    public abstract class BaseCrudController<TModel, TRepository, TService, TEditVM, TDetailsVM> : Controller
        where TModel : BaseDto
        where TRepository : IBaseRepository<TModel>
        where TService : IBaseCrudService<TModel, TRepository>
        where TEditVM : BaseVm, new()
        where TDetailsVM : BaseVm
    {
        protected readonly TService _service;
        protected readonly IMapper _mapper;

        protected BaseCrudController(TService service, IMapper mapper)
        {
            this._service = service;
            _mapper = mapper;
        }

        protected const int DefaultPageSize = 10;
        protected const int DefaultPageNumber = 1;
        protected const int MaxPageSize = 100;

        public virtual Task<string?> Validate(TEditVM editVM)
        {
            return Task.FromResult<string?>(null);
        }

        protected virtual Task<TEditVM> PrePopulateVMAsync(TEditVM editVM)
        {
            return Task.FromResult(editVM);
        }

        [HttpGet]
        public virtual async Task<IActionResult> List(int pageSize = DefaultPageSize, int pageNumber = DefaultPageNumber)
        {
            try
            {
                if (pageSize <= 0 || pageSize > MaxPageSize || pageNumber <= 0)
                {
                    return BadRequest(Constants.InvalidPagination);
                }

                var models = await _service.GetWithPaginationAsync(pageSize, pageNumber);
                var totalRecords = await _service.GetAllAsync();
                var totalPages = (int)Math.Ceiling((double)totalRecords.Count() / pageSize);

                var mappedModels = _mapper.Map<IEnumerable<TDetailsVM>>(models);

                ViewBag.TotalPages = totalPages;
                ViewBag.CurrentPage = pageNumber;

                return View(nameof(List), mappedModels);
            }
            catch (Exception ex)
            {
                // Log the exception (in a real application, you would use a logging service)
                return RedirectToAction("Error500", "Error");
            }
        }


        [HttpGet]
        public virtual async Task<IActionResult> Details(int id)
        {
            try
            {
                var model = await this._service.GetByIdIfExistsAsync(id);
                if (model == default)
                {
                    return RedirectToAction("Error404", "Error");
                }
                var mappedModel = _mapper.Map<TDetailsVM>(model);
                return View(mappedModel);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public virtual async Task<IActionResult> Create()
        {
            var editVM = await PrePopulateVMAsync(new TEditVM());
            return View(editVM);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Create(TEditVM editVM)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    editVM = await PrePopulateVMAsync(editVM);
                    return View(editVM);
                }

                var errors = await Validate(editVM);
                if (errors != null)
                {
                    ModelState.AddModelError("", errors);
                    editVM = await PrePopulateVMAsync(editVM);
                    return View(editVM);
                }

                var model = _mapper.Map<TModel>(editVM);
                await _service.SaveAsync(model);

                TempData["Success"] = $"{typeof(TModel).Name.ToFriendlyName()} was successfully created!";

                return RedirectToAction(nameof(List));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred during creation. Please try again.");
                editVM = await PrePopulateVMAsync(editVM);
                return View(editVM);
            }
        }
        [HttpGet]
        public virtual async Task<IActionResult> Edit(int? id)
        {
            try
            {
                if (id == null)
                {
                    return RedirectToAction("Error404", "Error");
                }
                var model = await this._service.GetByIdIfExistsAsync(id.Value);
                if (model == default)
                {
                    return RedirectToAction("Error404", "Error");
                }
                var mappedModel = _mapper.Map<TEditVM>(model);
                mappedModel = await PrePopulateVMAsync(mappedModel);

                return View(mappedModel);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error500", "Error");
            }
        }
        [HttpPost]
        public virtual async Task<IActionResult> Edit(int id, TEditVM editVM)
        {
            try
            {
                var errors = await Validate(editVM);
                if (errors != null)
                {
                    return View(editVM);
                }
                if (!await this._service.ExistsByIdAsync(id))
                {
                    return RedirectToAction("Error404", "Error");
                }
                var mappedModel = _mapper.Map<TModel>(editVM);
                await this._service.SaveAsync(mappedModel);

                TempData["Success"] = $"{typeof(TModel).Name.ToFriendlyName()} was successfully updated!";

                return await List();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred during editing. Please try again.");
                return View(editVM);
            }
        }
        [HttpGet]
        public virtual async Task<IActionResult> Delete(int? id)
        {
            try
            {
                if (id == null)
                {
                    return RedirectToAction("Error404", "Error");
                }
                var model = await this._service.GetByIdIfExistsAsync(id.Value);
                if (model == default)
                {
                    return RedirectToAction("Error404", "Error");
                }
                var mappedModel = _mapper.Map<TDetailsVM>(model);

                return View(mappedModel);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error500", "Error");
            }
        }
        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (!await this._service.ExistsByIdAsync(id))
                {
                    return RedirectToAction("Error404", "Error");
                }
                await this._service.DeleteAsync(id);

                TempData["Success"] = $"{typeof(TModel).Name.ToFriendlyName()} was successfully deleted!";

                return await List();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred during deletion. Please try again.";
                return RedirectToAction(nameof(List));
            }
        }
    }
}
