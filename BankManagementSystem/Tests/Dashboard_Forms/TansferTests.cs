using Xunit;
using FluentAssertions;
using System;
using System.Windows.Forms;
using BankManagementSystem.Dashboard_Forms;
using BankDatabaseAccess.EntityModel;

namespace BankManagementSystem.Tests.Dashboard_Forms
{
    public class TansferTests
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
            var form = new Tansfer(customer);

            // Assert
            form.Should().NotBeNull();
        }

        [Fact]
        public void Tansfer_IsFormType()
        {
            // Arrange
            var customer = new CustomerModel
            {
                Username = "testcustomer",
                Password = "TestPassword1",
                Email = "test@example.com"
            };

            // Act
            var form = new Tansfer(customer);

            // Assert
            form.Should().BeAssignableTo<Form>();
        }

        [Fact]
        public void RecentTransfers_ReturnsArrayWithTwoElements()
        {
            // Arrange
            var customer = new CustomerModel
            {
                Username = "testcustomer",
                Password = "TestPassword1",
                Email = "test@example.com"
            };
            var form = new Tansfer(customer);

            // Act
            var result = form.RecentTransfers;

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
        }

        [Fact]
        public void RecentTransfers_ContainsExpectedValues()
        {
            // Arrange
            var customer = new CustomerModel
            {
                Username = "testcustomer",
                Password = "TestPassword1",
                Email = "test@example.com"
            };
            var form = new Tansfer(customer);

            // Act
            var result = form.RecentTransfers;

            // Assert
            result[0].Should().Be(100);
            result[1].Should().Be(200);
        }

        [Fact]
        public void Constructor_WithNullCustomer_ThrowsException()
        {
            // Act
            Action act = () => new Tansfer(null);

            // Assert
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void Tansfer_CanBeInstantiated()
        {
            // Arrange
            var customer = new CustomerModel
            {
                Username = "testcustomer",
                Password = "TestPassword1",
                Email = "test@example.com"
            };

            // Act
            Action act = () => new Tansfer(customer);

            // Assert
            act.Should().NotThrow();
        }
    }
}
