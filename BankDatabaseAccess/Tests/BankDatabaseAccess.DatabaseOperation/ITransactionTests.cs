using Xunit;
using System;
using BankDatabaseAccess.DatabaseOperation;
using BankDatabaseAccess.EntityModel;

namespace BankDatabaseAccess.DatabaseOperation.Tests
{
    public class ITransactionTests
    {
        // Mock implementation for testing interface
        private class MockTransaction : ITransaction
        {
            public int UpdateBalance(PersonModel personModel, decimal amount)
            {
                if (personModel == null)
                    throw new ArgumentNullException(nameof(personModel));
                
                if (amount < 0)
                    return -1;
                
                return 1; // Success
            }
        }

        [Fact]
        public void ITransaction_ShouldBeInterface()
        {
            // Arrange
            var transactionType = typeof(ITransaction);

            // Act & Assert
            Assert.True(transactionType.IsInterface);
        }

        [Fact]
        public void ITransaction_ShouldHaveUpdateBalanceMethod()
        {
            // Arrange
            var transactionType = typeof(ITransaction);

            // Act
            var method = transactionType.GetMethod("UpdateBalance");

            // Assert
            Assert.NotNull(method);
            Assert.Equal(typeof(int), method!.ReturnType);
        }

        [Fact]
        public void UpdateBalanceMethod_ShouldHaveTwoParameters()
        {
            // Arrange
            var transactionType = typeof(ITransaction);
            var method = transactionType.GetMethod("UpdateBalance");

            // Act
            var parameters = method!.GetParameters();

            // Assert
            Assert.Equal(2, parameters.Length);
        }

        [Fact]
        public void UpdateBalanceMethod_FirstParameter_ShouldBePersonModel()
        {
            // Arrange
            var transactionType = typeof(ITransaction);
            var method = transactionType.GetMethod("UpdateBalance");

            // Act
            var parameters = method!.GetParameters();

            // Assert
            Assert.Equal(typeof(PersonModel), parameters[0].ParameterType);
            Assert.Equal("personModel", parameters[0].Name);
        }

        [Fact]
        public void UpdateBalanceMethod_SecondParameter_ShouldBeDecimal()
        {
            // Arrange
            var transactionType = typeof(ITransaction);
            var method = transactionType.GetMethod("UpdateBalance");

            // Act
            var parameters = method!.GetParameters();

            // Assert
            Assert.Equal(typeof(decimal), parameters[1].ParameterType);
            Assert.Equal("amount", parameters[1].Name);
        }

        [Fact]
        public void MockTransaction_ShouldImplementITransaction()
        {
            // Arrange & Act
            var mockTransaction = new MockTransaction();

            // Assert
            Assert.IsAssignableFrom<ITransaction>(mockTransaction);
        }

        [Fact]
        public void UpdateBalance_WithValidPersonModelAndAmount_ShouldReturnSuccess()
        {
            // Arrange
            ITransaction transaction = new MockTransaction();
            var personModel = new PersonModel { Username = "testuser" };
            decimal amount = 100.50m;

            // Act
            var result = transaction.UpdateBalance(personModel, amount);

            // Assert
            Assert.Equal(1, result);
        }

        [Fact]
        public void UpdateBalance_WithNullPersonModel_ShouldThrowException()
        {
            // Arrange
            ITransaction transaction = new MockTransaction();
            PersonModel? personModel = null;
            decimal amount = 100.50m;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => transaction.UpdateBalance(personModel!, amount));
        }

        [Fact]
        public void UpdateBalance_WithNegativeAmount_ShouldReturnError()
        {
            // Arrange
            ITransaction transaction = new MockTransaction();
            var personModel = new PersonModel { Username = "testuser" };
            decimal amount = -100.50m;

            // Act
            var result = transaction.UpdateBalance(personModel, amount);

            // Assert
            Assert.Equal(-1, result);
        }

        [Fact]
        public void UpdateBalance_WithZeroAmount_ShouldReturnSuccess()
        {
            // Arrange
            ITransaction transaction = new MockTransaction();
            var personModel = new PersonModel { Username = "testuser" };
            decimal amount = 0m;

            // Act
            var result = transaction.UpdateBalance(personModel, amount);

            // Assert
            Assert.Equal(1, result);
        }

        [Theory]
        [InlineData(100.00)]
        [InlineData(1000.50)]
        [InlineData(0.01)]
        [InlineData(999999.99)]
        public void UpdateBalance_WithVariousPositiveAmounts_ShouldReturnSuccess(decimal amount)
        {
            // Arrange
            ITransaction transaction = new MockTransaction();
            var personModel = new PersonModel { Username = "testuser" };

            // Act
            var result = transaction.UpdateBalance(personModel, amount);

            // Assert
            Assert.Equal(1, result);
        }

        [Fact]
        public void UpdateBalance_WithCustomerModel_ShouldWork()
        {
            // Arrange
            ITransaction transaction = new MockTransaction();
            var customerModel = new CustomerModel 
            { 
                Username = "customer1",
                Balance = 500.00m
            };
            decimal amount = 100.00m;

            // Act
            var result = transaction.UpdateBalance(customerModel, amount);

            // Assert
            Assert.Equal(1, result);
        }

        [Fact]
        public void UpdateBalance_WithEmployeeModel_ShouldWork()
        {
            // Arrange
            ITransaction transaction = new MockTransaction();
            var employeeModel = new EmployeeModel 
            { 
                Username = "employee1"
            };
            decimal amount = 200.00m;

            // Act
            var result = transaction.UpdateBalance(employeeModel, amount);

            // Assert
            Assert.Equal(1, result);
        }

        [Fact]
        public void ITransaction_ShouldHaveOnlyOneMethod()
        {
            // Arrange
            var transactionType = typeof(ITransaction);

            // Act
            var methods = transactionType.GetMethods();

            // Assert
            Assert.Single(methods);
        }

        [Fact]
        public void UpdateBalance_ReturnType_ShouldBeInt()
        {
            // Arrange
            var transactionType = typeof(ITransaction);
            var method = transactionType.GetMethod("UpdateBalance");

            // Act
            var returnType = method!.ReturnType;

            // Assert
            Assert.Equal(typeof(int), returnType);
        }
    }
}
