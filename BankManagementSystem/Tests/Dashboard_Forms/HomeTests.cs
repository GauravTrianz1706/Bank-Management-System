using Xunit;
using FluentAssertions;
using System;
using System.Windows.Forms;
using BankManagementSystem.Dashboard_Forms;
using BankDatabaseAccess.EntityModel;

namespace BankManagementSystem.Tests.Dashboard_Forms
{
    public class HomeTests
    {
        [Fact]
        public void Constructor_WithValidCustomer_InitializesForm()
        {
            // Arrange
            var customer = new CustomerModel
            {
                Username = "testcustomer",
                Password = "TestPassword1",
                Email = "test@example.com"
            };

            // Act
            var form = new Home(customer);

            // Assert
            form.Should().NotBeNull();
        }

        [Fact]
        public void Home_IsFormType()
        {
            // Arrange
            var customer = new CustomerModel
            {
                Username = "testcustomer",
                Password = "TestPassword1",
                Email = "test@example.com"
            };

            // Act
            var form = new Home(customer);

            // Assert
            form.Should().BeAssignableTo<Form>();
        }

        [Fact]
        public void Constructor_WithNullCustomer_ThrowsException()
        {
            // Act
            Action act = () => new Home(null);

            // Assert
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void Home_CanBeInstantiated()
        {
            // Arrange
            var customer = new CustomerModel
            {
                Username = "testcustomer",
                Password = "TestPassword1",
                Email = "test@example.com"
            };

            // Act
            Action act = () => new Home(customer);

            // Assert
            act.Should().NotThrow();
        }
    }
}
