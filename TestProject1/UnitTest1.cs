using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Data;
using System.Linq;
using TO_DO.Data;
using TO_DO.DTOs;
using TO_DO.DTOs.Pagination;
using TO_DO.Models;
using TO_DO.Servises;

namespace TestProject1
{
    public class Calculator
    {
        public int Add(int a, int b) => a + b;
    }

    public class FakeEmailSimulator : IEmailSender
    {
        public Task SendEmail(string to, string subject, string body)
        {
            return Task.CompletedTask;
        }
    }

    public class UnitTest1
    {
        public static IEnumerable<object[]> AddData()
        {
            yield return new object[] { 1, 2, 3 };
            yield return new object[] { 2, 2, 4 };
            yield return new object[] { 3, 2, 5 };
        } 


        [Theory]
        //[InlineData(1,2,3)]
        //[InlineData(0,0,0)]
        //[InlineData(2,2,4)]

        [MemberData(nameof(AddData))]
        public  void Add_ReturnResult(int a, int b, int expectResult)
        {
            var calculator = new Calculator();
            var actualResult=calculator.Add(a, b);
            Assert.Equal(expectResult, actualResult);
        }

        [Fact]
        public async Task GetToDoItem_ReturnToDoItemWhichBelongToUser()
        {
            var dbContext = new ToDoDbContext(new DbContextOptionsBuilder<ToDoDbContext>().UseInMemoryDatabase("test").Options);

            var user = dbContext.Users.Add(new AppUser
            {
                UserName="Test",
                Email="Test12345@gmail.com"
            }).Entity;

            var emailSender = new FakeEmailSimulator();

            var createdToDoItem = dbContext.ToDoItems.Add(new ToDoItem
            {
                Text = "Test",
                UserId = user.Id
            }).Entity;

            await dbContext.SaveChangesAsync();

            var service = new ToDoService(dbContext,emailSender);

            var retrivedToDoItem = await service.GetToDoItem(user.Id,createdToDoItem.Id);

            Assert.NotNull(retrivedToDoItem);
        }

        [Fact]
        public async Task GetToDoItem_ReturnToDoItemWhichNotBelongToUser()
        {
            var dbContext = new ToDoDbContext(new DbContextOptionsBuilder<ToDoDbContext>().UseInMemoryDatabase("test").Options);

            var user = dbContext.Users.Add(new AppUser
            {
                UserName = "Test",
                Email = "Test12345@gmail.com"
            }).Entity;
            var emailSender = new FakeEmailSimulator();

            var anotherUser = dbContext.Users.Add(new AppUser
            {
                UserName = "Test",
                Email = "Test12345@gmail.com"
            }).Entity;

            var createdToDoItem = dbContext.ToDoItems.Add(new ToDoItem
            {
                Text = "Test",
                UserId = user.Id
            }).Entity;

            await dbContext.SaveChangesAsync();

            var service = new ToDoService(dbContext,emailSender);

            var retrivedToDoItem = await service.GetToDoItem(anotherUser.Id, createdToDoItem.Id);

            Assert.Null(retrivedToDoItem);
        }

        [Fact]
        public async Task GetToDoItem_ReturnPaginatedToDoItemWhichBelongToUser()
        {
            var dbContext = new ToDoDbContext(new DbContextOptionsBuilder<ToDoDbContext>().UseInMemoryDatabase("test").Options);

            var user = dbContext.Users.Add(new AppUser
            {
                UserName = "Test",
                Email = "Test12345@gmail.com"
            }).Entity;
            var emailSender = new FakeEmailSimulator();

            Enumerable
                .Range(1, 5)
                .Select(i => new ToDoItem
                {
                    Text = $"Test {i}",
                    UserId = user.Id
                }).ToList()
                .ForEach(todo=>dbContext.ToDoItems.Add(todo));           

            await dbContext.SaveChangesAsync();

            var service = new ToDoService(dbContext, emailSender);

            var retrivedToDOItems = await service.GetToDoItems(user.Id, 1, 3, null, null);

            retrivedToDOItems.Should().NotBeNull();
            retrivedToDOItems.Meta.Should().BeEquivalentTo(new PaginationMeta(1, 3, 5));
            retrivedToDOItems.Items.Should().HaveCount(3)
                .And.ContainSingle(item=>item.Text=="Test 1")
                .And.ContainSingle(item=>item.Text=="Test 2")
                .And.ContainSingle(item=>item.Text=="Test 3");
        }

        [Fact]
        public async Task CreateToDoItem_CreateNewItem_AndSendEmailNotification()
        {
            var dbContext = new ToDoDbContext(new DbContextOptionsBuilder<ToDoDbContext>().UseInMemoryDatabase("test").Options);

            var user = dbContext.Users.Add(new AppUser
            {
                UserName = "Test",
                Email = "Test12345@gmail.com"
            }).Entity;

            var createdToDoItem =new  CreateToDoItemRequest
            {
                Text = "Test"
            };
            await dbContext.SaveChangesAsync();

            var emailSender = new Mock<IEmailSender>();
            var service=new ToDoService(dbContext,emailSender.Object);
            var retrivedItems = await service.CreateTodoItem(user.Id, createdToDoItem);
            retrivedItems.Should().NotBeNull();
            emailSender.Verify(x=>x.SendEmail(user.Email,It.IsAny<string>(), It.IsAny<string>()));
        }
    }
}