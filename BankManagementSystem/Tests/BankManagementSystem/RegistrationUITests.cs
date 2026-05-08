using Xunit;
using FluentAssertions;
using System;
using System.Windows.Forms;
using BankManagementSystem;

namespace BankManagementSystem.Tests
{
    public class RegistrationUITests
    {
        [Fact]
        public void Constructor_InitializesForm()
        {
            // Act
            var form = new RegistrationUI();

            // Assert
            form.Should().NotBeNull();
        }

        [Fact]
        public void RegistrationUI_IsFormType()
        {
            // Act
            var form = new RegistrationUI();

            // Assert
            form.Should().BeAssignableTo<Form>();
        }

        [Fact]
        public void Constructor_WhenUserIsCustomer_SetsTitleToCustomerRegistration()
        {
            // Arrange
            UILogics.User = UILogics.UserType.Customer;

            // Act
            var form = new RegistrationUI();

            // Assert
            form.Text.Should().Be("Registration For Customer Account");
        }

        [Fact]
        public void Constructor_WhenUserIsEmployee_SetsTitleToEmployeeRegistration()
        {
            // Arrange
            UILogics.User = UILogics.UserType.Employee;

            // Act
            var form = new RegistrationUI();

            // Assert
            form.Text.Should().Be("Registration For Employee Account");
        }

        [Fact]
        public void RegistrationUI_CanBeInstantiated()
        {
            // Act
            Action act = () => new RegistrationUI();

            // Assert
            act.Should().NotThrow();
        }
    }
}
