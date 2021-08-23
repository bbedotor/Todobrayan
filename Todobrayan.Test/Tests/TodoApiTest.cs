using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Todobrayan.Common.Models;
using Todobrayan.Functions.Functions;
using Todobrayan.Test.Helpers;
using Xunit;

namespace Todobrayan.Test.Tests
{
    public class TodoApiTest
    {
        private readonly ILogger logger = TestFactory.CreateLogger();
        
        [Fact]
        public async void CreateTodo_Should_Return_200()
        {
            //Arrenge

            MockCloudTableTodos mocTodos =  new MockCloudTableTodos(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            Todo todoRequest = TestFactory.getTodoRequest();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(todoRequest);

            //act

            IActionResult response = await TodoApi.CreateTodo(request, mocTodos, logger);


            // assert

            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);

        }

        [Fact]
        public async void UpdateTodo_Should_Return_200()
        {
            //Arrenge

            MockCloudTableTodos mocTodos = new MockCloudTableTodos(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            Todo todoRequest = TestFactory.getTodoRequest();
            Guid todoId = Guid.NewGuid();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(todoId, todoRequest);

            //act

            IActionResult response = await TodoApi.UpdateTodo(request, mocTodos,todoId.ToString(), logger);


            // assert

            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);

        }
    }
}
