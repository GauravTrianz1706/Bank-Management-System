using Xunit;
using FluentAssertions;
using System;
using System.Windows.Forms;
using BankManagementSystem;

namespace BankManagementSystem.Tests
{
    public class LoginUITests
    {
        [Fact]
        public void Constructor_InitializesForm()
        {
            // Act
            var form = new LoginUI();

            // Assert
            form.Should().NotBeNull();
        }

        [Fact]
        public void LoginUI_IsFormType()
        {
            // Act
            var form = new LoginUI();

            // Assert
            form.Should().BeAssignableTo<Form>();
        }

        [Fact]
        public void Constructor_WhenUserIsCustomer_SetsTitleToCustomerLogin()
        {
            // Arrange
            UILogics.User = UILogics.UserType.Customer;

            // Act
            var form = new LoginUI();

            // Assert
            form.Text.Should().Be("Login As Customer");
        }

        [Fact]
        public void Constructor_WhenUserIsEmployee_SetsTitleToEmployeeLogin()
        {
            // Arrange
            UILogics.User = UILogics.UserType.Employee;

            // Act
            var form = new LoginUI();

            // Assert
            form.Text.Should().Be("Log In as Employee");
        }

        [Fact]
        public void LoginUI_CanBeInstantiated()
        {
            // Act
            Action act = () => new LoginUI();

            // Assert
            act.Should().NotThrow();
        }
    }
}
