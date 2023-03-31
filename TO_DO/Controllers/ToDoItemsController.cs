﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TO_DO.DTOs;
using TO_DO.DTOs.Pagination;
using TO_DO.Models;
using TO_DO.Servises;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TO_DO.Controllers;
[Authorize]
[Route("api/[controller]")]
[ApiController]
public class ToDoItemsController : ControllerBase
{
    readonly IToDoService _toDoService;

    public ToDoItemsController(IToDoService toDoService)
    {
        _toDoService = toDoService;
    }

    // GET: api/<ToDoItemsController>
    [HttpGet]
    public async Task<ActionResult<PaginatedListDto<ToDoItemDto>>>GetList(
        [FromQuery] ToDoQueryFilters filters,
        [FromQuery] PaginationRequest request
        )
    {
        return await _toDoService.GetToDoItems(
            request.Page, 
            request.PageSize,
            filters.Search,
            filters.IsCompleted
            );
    }

    // GET api/<ToDoItemsController>/5
    /// <summary>
    /// Get ToDo Item
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ToDoItemDto>> Get(int id)
    {
        var item = await _toDoService.GetToDoItem(id);
        return item != null 
            ? item // Ok(item)
            : NotFound();
    }

    // POST api/<ToDoItemsController>
    /// <summary>
    /// Create ToDo Item
    /// </summary>
    /// <param name="request"></param>
    /// <response code="201">Success</response>
    /// <response code="409">Task already created</response>
    /// <response code="403">Forbiden</response>
    [HttpPost]
    public async Task<ActionResult<ToDoItemDto>> Create([FromBody] CreateToDoItemRequest request)
    {
        var createdItem = await _toDoService.CreateTodoItem(request);
        return createdItem;
    }

    // Patch api/<ToDoItemsController>/5/status
    [HttpPatch("{id}/status")]
    public async Task<ActionResult<ToDoItemDto>> Patch(int id, [FromBody] bool isCompleted)
    {
        var todoItem = await _toDoService.ChangeTodoItemStatus(id, isCompleted);
        return todoItem != null
            ?todoItem
            :NotFound();
    }       
}

// MVC

// CREATE:
// GET      /products/create -> html
// POST     /products/create -> html

// UPDATE:
// GET      /products/update{id} -> html
// POST     /products/update{id} -> html

// DELETE:
// GET      /products/delete{id} -> html
// POST     /products/delete{id} -> html

// GET ALL
// GET      /products/index -> html


// GET ONE
// GET      /products/{id}/details -> html


// Web API:

// CREATE:
// POST         /products -> json

// UPDATE:
// PUT          /products/{id} -> json

// DELETE:
// DELETE       /products/{id} -> json


// GET ALL
// GET          /products -> json


// GET ONE
// GET          /products/{id} -> json