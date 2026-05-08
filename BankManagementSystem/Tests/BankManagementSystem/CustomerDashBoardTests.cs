using Xunit;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using BankManagementSystem;
using BankDatabaseAccess.EntityModel;

namespace BankManagementSystem.Tests
{
    public class CustomerDashBoardTests
    {
        [Fact]
        public void Constructor_WithValidCustomer_InitializesForm()
        {
            // Arrange
            var customer = new CustomerModel
            {
                Username = "testuser",
                Password = "TestPassword1"
            };

            // Act
            var form = new CustomerDashBoard(customer);

            // Assert
            form.Should().NotBeNull();
        }

        [Fact]
        public void CustomerDashBoard_IsFormType()
        {
            // Arrange
            var customer = new CustomerModel
            {
                Username = "testuser",
                Password = "TestPassword1"
            };

            // Act
            var form = new CustomerDashBoard(customer);

            // Assert
            form.Should().BeAssignableTo<Form>();
        }

        [Fact]
        public void Constructor_InitializesNavigationHistory()
        {
            // Arrange
            var customer = new CustomerModel
            {
                Username = "testuser",
                Password = "TestPassword1"
            };

            // Act
            var form = new CustomerDashBoard(customer);

            // Assert
            form.NavigationHistory.Should().NotBeNull();
            form.NavigationHistory.Should().BeOfType<List<string>>();
        }

        [Fact]
        public void NavigationHistory_IsEmptyOnInitialization()
        {
            // Arrange
            var customer = new CustomerModel
            {
                Username = "testuser",
                Password = "TestPassword1"
            };

            // Act
            var form = new CustomerDashBoard(customer);

            // Assert
            form.NavigationHistory.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_WithNullCustomer_ThrowsException()
        {
            // Act
            Action act = () => new CustomerDashBoard(null);

            // Assert
            act.Should().Throw<Exception>();
        }
    }
}
