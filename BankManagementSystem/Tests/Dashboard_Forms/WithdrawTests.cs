using Xunit;
using FluentAssertions;
using System;
using System.Windows.Forms;
using BankManagementSystem.Dashboard_Forms;

namespace BankManagementSystem.Tests.Dashboard_Forms
{
    public class WithdrawTests
    {
        [Fact]
        public void Constructor_InitializesForm()
        {
            // Act
            var form = new Withdraw();

            // Assert
            form.Should().NotBeNull();
        }

        [Fact]
        public void Withdraw_IsFormType()
        {
            // Act
            var form = new Withdraw();

            // Assert
            form.Should().BeAssignableTo<Form>();
        }

        [Fact]
        public void Withdraw_CanBeInstantiated()
        {
            // Act
            Action act = () => new Withdraw();

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void Constructor_DoesNotThrowException()
        {
            // Act
            Action act = () => new Withdraw();

            // Assert
            act.Should().NotThrow();
        }
    }
}
