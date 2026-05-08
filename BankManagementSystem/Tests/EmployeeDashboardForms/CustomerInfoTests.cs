using Xunit;
using FluentAssertions;
using System;
using System.Windows.Forms;
using BankManagementSystem.EmployeeDashboardForms;

namespace BankManagementSystem.Tests.EmployeeDashboardForms
{
    public class CustomerInfoTests
    {
        [Fact]
        public void Constructor_InitializesForm()
        {
            // Act
            var form = new CustomerInfo();

            // Assert
            form.Should().NotBeNull();
        }

        [Fact]
        public void CustomerInfo_IsFormType()
        {
            // Act
            var form = new CustomerInfo();

            // Assert
            form.Should().BeAssignableTo<Form>();
        }

        [Fact]
        public void CustomerInfo_CanBeInstantiated()
        {
            // Act
            Action act = () => new CustomerInfo();

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void Constructor_DoesNotThrowException()
        {
            // Act
            Action act = () => new CustomerInfo();

            // Assert
            act.Should().NotThrow();
        }
    }
}
