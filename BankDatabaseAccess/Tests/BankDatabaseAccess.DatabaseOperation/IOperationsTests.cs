using Xunit;
using System;
using BankDatabaseAccess.DatabaseOperation;
using BankDatabaseAccess.EntityModel;

namespace BankDatabaseAccess.DatabaseOperation.Tests
{
    public class IOperationsTests
    {
        // Mock implementation for testing interface
        private class MockOperations : IOperations
        {
            public int Insert(PersonModel personModel)
            {
                if (personModel == null)
                    throw new ArgumentNullException(nameof(personModel));
                
                if (string.IsNullOrEmpty(personModel.Username))
                    return -1;
                
                return 1; // Success
            }

            public int Update(PersonModel personModel)
            {
                if (personModel == null)
                    throw new ArgumentNullException(nameof(personModel));
                
                if (string.IsNullOrEmpty(personModel.Username))
                    return -1;
                
                return 1; // Success
            }

            public int Delete(PersonModel personModel)
            {
                if (personModel == null)
                    throw new ArgumentNullException(nameof(personModel));
                
                if (string.IsNullOrEmpty(personModel.Username))
                    return -1;
                
                return 1; // Success
            }
        }

        [Fact]
        public void IOperations_ShouldBeInterface()
        {
            // Arrange
            var operationsType = typeof(IOperations);

            // Act & Assert
            Assert.True(operationsType.IsInterface);
        }

        [Fact]
        public void IOperations_ShouldHaveInsertMethod()
        {
            // Arrange
            var operationsType = typeof(IOperations);

            // Act
            var method = operationsType.GetMethod("Insert");

            // Assert
            Assert.NotNull(method);
            Assert.Equal(typeof(int), method!.ReturnType);
        }

        [Fact]
        public void IOperations_ShouldHaveUpdateMethod()
        {
            // Arrange
            var operationsType = typeof(IOperations);

            // Act
            var method = operationsType.GetMethod("Update");

            // Assert
            Assert.NotNull(method);
            Assert.Equal(typeof(int), method!.ReturnType);
        }

        [Fact]
        public void IOperations_ShouldHaveDeleteMethod()
        {
            // Arrange
            var operationsType = typeof(IOperations);

            // Act
            var method = operationsType.GetMethod("Delete");

            // Assert
            Assert.NotNull(method);
            Assert.Equal(typeof(int), method!.ReturnType);
        }

        [Fact]
        public void InsertMethod_ShouldHaveOneParameter()
        {
            // Arrange
            var operationsType = typeof(IOperations);
            var method = operationsType.GetMethod("Insert");

            // Act
            var parameters = method!.GetParameters();

            // Assert
            Assert.Single(parameters);
        }

        [Fact]
        public void InsertMethod_Parameter_ShouldBePersonModel()
        {
            // Arrange
            var operationsType = typeof(IOperations);
            var method = operationsType.GetMethod("Insert");

            // Act
            var parameters = method!.GetParameters();

            // Assert
            Assert.Equal(typeof(PersonModel), parameters[0].ParameterType);
            Assert.Equal("personModel", parameters[0].Name);
        }

        [Fact]
        public void UpdateMethod_ShouldHaveOneParameter()
        {
            // Arrange
            var operationsType = typeof(IOperations);
            var method = operationsType.GetMethod("Update");

            // Act
            var parameters = method!.GetParameters();

            // Assert
            Assert.Single(parameters);
        }

        [Fact]
        public void UpdateMethod_Parameter_ShouldBePersonModel()
        {
            // Arrange
            var operationsType = typeof(IOperations);
            var method = operationsType.GetMethod("Update");

            // Act
            var parameters = method!.GetParameters();

            // Assert
            Assert.Equal(typeof(PersonModel), parameters[0].ParameterType);
            Assert.Equal("personModel", parameters[0].Name);
        }

        [Fact]
        public void DeleteMethod_ShouldHaveOneParameter()
        {
            // Arrange
            var operationsType = typeof(IOperations);
            var method = operationsType.GetMethod("Delete");

            // Act
            var parameters = method!.GetParameters();

            // Assert
            Assert.Single(parameters);
        }

        [Fact]
        public void DeleteMethod_Parameter_ShouldBePersonModel()
        {
            // Arrange
            var operationsType = typeof(IOperations);
            var method = operationsType.GetMethod("Delete");

            // Act
            var parameters = method!.GetParameters();

            // Assert
            Assert.Equal(typeof(PersonModel), parameters[0].ParameterType);
            Assert.Equal("personModel", parameters[0].Name);
        }

        [Fact]
        public void MockOperations_ShouldImplementIOperations()
        {
            // Arrange & Act
            var mockOperations = new MockOperations();

            // Assert
            Assert.IsAssignableFrom<IOperations>(mockOperations);
        }

        [Fact]
        public void Insert_WithValidPersonModel_ShouldReturnSuccess()
        {
            // Arrange
            IOperations operations = new MockOperations();
            var personModel = new PersonModel { Username = "testuser" };

            // Act
            var result = operations.Insert(personModel);

            // Assert
            Assert.Equal(1, result);
        }

        [Fact]
        public void Insert_WithNullPersonModel_ShouldThrowException()
        {
            // Arrange
            IOperations operations = new MockOperations();
            PersonModel? personModel = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => operations.Insert(personModel!));
        }

        [Fact]
        public void Insert_WithEmptyUsername_ShouldReturnError()
        {
            // Arrange
            IOperations operations = new MockOperations();
            var personModel = new PersonModel { Username = "" };

            // Act
            var result = operations.Insert(personModel);

            // Assert
            Assert.Equal(-1, result);
        }

        [Fact]
        public void Update_WithValidPersonModel_ShouldReturnSuccess()
        {
            // Arrange
            IOperations operations = new MockOperations();
            var personModel = new PersonModel { Username = "testuser" };

            // Act
            var result = operations.Update(personModel);

            // Assert
            Assert.Equal(1, result);
        }

        [Fact]
        public void Update_WithNullPersonModel_ShouldThrowException()
        {
            // Arrange
            IOperations operations = new MockOperations();
            PersonModel? personModel = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => operations.Update(personModel!));
        }

        [Fact]
        public void Update_WithEmptyUsername_ShouldReturnError()
        {
            // Arrange
            IOperations operations = new MockOperations();
            var personModel = new PersonModel { Username = "" };

            // Act
            var result = operations.Update(personModel);

            // Assert
            Assert.Equal(-1, result);
        }

        [Fact]
        public void Delete_WithValidPersonModel_ShouldReturnSuccess()
        {
            // Arrange
            IOperations operations = new MockOperations();
            var personModel = new PersonModel { Username = "testuser" };

            // Act
            var result = operations.Delete(personModel);

            // Assert
            Assert.Equal(1, result);
        }

        [Fact]
        public void Delete_WithNullPersonModel_ShouldThrowException()
        {
            // Arrange
            IOperations operations = new MockOperations();
            PersonModel? personModel = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => operations.Delete(personModel!));
        }

        [Fact]
        public void Delete_WithEmptyUsername_ShouldReturnError()
        {
            // Arrange
            IOperations operations = new MockOperations();
            var personModel = new PersonModel { Username = "" };

            // Act
            var result = operations.Delete(personModel);

            // Assert
            Assert.Equal(-1, result);
        }

        [Fact]
        public void Insert_WithCustomerModel_ShouldWork()
        {
            // Arrange
            IOperations operations = new MockOperations();
            var customerModel = new CustomerModel 
            { 
                Username = "customer1",
                Balance = 1000.00m
            };

            // Act
            var result = operations.Insert(customerModel);

            // Assert
            Assert.Equal(1, result);
        }

        [Fact]
        public void Insert_WithEmployeeModel_ShouldWork()
        {
            // Arrange
            IOperations operations = new MockOperations();
            var employeeModel = new EmployeeModel 
            { 
                Username = "employee1"
            };

            // Act
            var result = operations.Insert(employeeModel);

            // Assert
            Assert.Equal(1, result);
        }

        [Fact]
        public void Update_WithCustomerModel_ShouldWork()
        {
            // Arrange
            IOperations operations = new MockOperations();
            var customerModel = new CustomerModel 
            { 
                Username = "customer1",
                Email = "customer@example.com"
            };

            // Act
            var result = operations.Update(customerModel);

            // Assert
            Assert.Equal(1, result);
        }

        [Fact]
        public void Delete_WithCustomerModel_ShouldWork()
        {
            // Arrange
            IOperations operations = new MockOperations();
            var customerModel = new CustomerModel 
            { 
                Username = "customer1"
            };

            // Act
            var result = operations.Delete(customerModel);

            // Assert
            Assert.Equal(1, result);
        }

        [Fact]
        public void IOperations_ShouldHaveThreeMethods()
        {
            // Arrange
            var operationsType = typeof(IOperations);

            // Act
            var methods = operationsType.GetMethods();

            // Assert
            Assert.Equal(3, methods.Length);
        }

        [Fact]
        public void AllMethods_ShouldReturnInt()
        {
            // Arrange
            var operationsType = typeof(IOperations);

            // Act
            var insertMethod = operationsType.GetMethod("Insert");
            var updateMethod = operationsType.GetMethod("Update");
            var deleteMethod = operationsType.GetMethod("Delete");

            // Assert
            Assert.Equal(typeof(int), insertMethod!.ReturnType);
            Assert.Equal(typeof(int), updateMethod!.ReturnType);
            Assert.Equal(typeof(int), deleteMethod!.ReturnType);
        }
    }
}
