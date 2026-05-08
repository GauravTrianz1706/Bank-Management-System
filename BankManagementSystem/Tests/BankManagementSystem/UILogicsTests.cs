using Xunit;
using Moq;
using FluentAssertions;
using System;
using System.Drawing;
using System.Windows.Forms;
using BankManagementSystem;
using BankDatabaseAccess.EntityModel;

namespace BankManagementSystem.Tests
{
    public class UILogicsTests
    {
        [Fact]
        public void IsCustomer_WhenUserTypeIsCustomer_ReturnsTrue()
        {
            // Arrange
            UILogics.User = UILogics.UserType.Customer;

            // Act
            var result = UILogics.IsCustomer();

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void IsCustomer_WhenUserTypeIsEmployee_ReturnsFalse()
        {
            // Arrange
            UILogics.User = UILogics.UserType.Employee;

            // Act
            var result = UILogics.IsCustomer();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void IsEmployee_WhenUserTypeIsEmployee_ReturnsTrue()
        {
            // Arrange
            UILogics.User = UILogics.UserType.Employee;

            // Act
            var result = UILogics.IsEmployee();

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void IsEmployee_WhenUserTypeIsCustomer_ReturnsFalse()
        {
            // Arrange
            UILogics.User = UILogics.UserType.Customer;

            // Act
            var result = UILogics.IsEmployee();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void EnterUpdate_WhenTextMatchesPlaceholder_ClearsTextAndChangesColor()
        {
            // Arrange
            var textBox = new TextBox
            {
                Text = "Placeholder",
                ForeColor = Color.DarkGray
            };

            // Act
            UILogics.EnterUpdate(textBox, "Placeholder");

            // Assert
            textBox.Text.Should().BeEmpty();
            textBox.ForeColor.Should().Be(Color.Black);
        }

        [Fact]
        public void EnterUpdate_WhenTextDoesNotMatchPlaceholder_DoesNotChangeText()
        {
            // Arrange
            var textBox = new TextBox
            {
                Text = "UserInput",
                ForeColor = Color.Black
            };

            // Act
            UILogics.EnterUpdate(textBox, "Placeholder");

            // Assert
            textBox.Text.Should().Be("UserInput");
        }

        [Fact]
        public void LeaveUpdate_WhenTextIsEmpty_SetsPlaceholderAndChangesColor()
        {
            // Arrange
            var textBox = new TextBox
            {
                Text = "",
                ForeColor = Color.Black
            };

            // Act
            UILogics.LeaveUpdate(textBox, "Placeholder");

            // Assert
            textBox.Text.Should().Be("Placeholder");
            textBox.ForeColor.Should().Be(Color.DarkGray);
        }

        [Fact]
        public void LeaveUpdate_WhenTextIsNotEmpty_DoesNotChangePlaceholder()
        {
            // Arrange
            var textBox = new TextBox
            {
                Text = "UserInput",
                ForeColor = Color.Black
            };

            // Act
            UILogics.LeaveUpdate(textBox, "Placeholder");

            // Assert
            textBox.Text.Should().Be("UserInput");
        }

        [Fact]
        public void LoadForm_AddsFormToPanel()
        {
            // Arrange
            var panel = new Panel();
            var form = new Form();

            // Act
            UILogics.LoadForm(panel, form);

            // Assert
            panel.Controls.Count.Should().Be(1);
            panel.Controls[0].Should().Be(form);
        }

        [Fact]
        public void LoadForm_RemovesExistingControlBeforeAddingNewForm()
        {
            // Arrange
            var panel = new Panel();
            var existingControl = new Button();
            panel.Controls.Add(existingControl);
            var form = new Form();

            // Act
            UILogics.LoadForm(panel, form);

            // Assert
            panel.Controls.Count.Should().Be(1);
            panel.Controls[0].Should().Be(form);
        }

        [Fact]
        public void SetPrecision_ReturnsNumberFormatInfoWithCorrectDecimalDigits()
        {
            // Arrange
            int precision = 2;

            // Act
            var result = UILogics.SetPrecision(precision);

            // Assert
            result.Should().NotBeNull();
            result.NumberDecimalDigits.Should().Be(2);
        }

        [Fact]
        public void PasswordChekcer_WithEmptyPassword_ReturnsFalse()
        {
            // Arrange
            var textBox = new TextBox { Text = "" };

            // Act
            var result = UILogics.PasswordChekcer(textBox);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void PasswordChekcer_WithShortPassword_ReturnsFalse()
        {
            // Arrange
            var textBox = new TextBox { Text = "Short1" };

            // Act
            var result = UILogics.PasswordChekcer(textBox);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void PasswordChekcer_WithNoUppercase_ReturnsFalse()
        {
            // Arrange
            var textBox = new TextBox { Text = "lowercase123" };

            // Act
            var result = UILogics.PasswordChekcer(textBox);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void PasswordChekcer_WithValidPassword_ReturnsTrue()
        {
            // Arrange
            var textBox = new TextBox { Text = "ValidPassword1" };

            // Act
            var result = UILogics.PasswordChekcer(textBox);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void PhoneNumberCkecker_WithValidPhoneNumber_ReturnsTrue()
        {
            // Arrange
            var textBox = new TextBox { Text = "1234567890" };

            // Act
            var result = UILogics.PhoneNumberCkecker(textBox);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void PhoneNumberCkecker_WithInvalidPhoneNumber_ReturnsFalse()
        {
            // Arrange
            var textBox = new TextBox { Text = "123abc456" };

            // Act
            var result = UILogics.PhoneNumberCkecker(textBox);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void NidCkecker_WithValidNid_ReturnsTrue()
        {
            // Arrange
            var textBox = new TextBox { Text = "1234567890123" };

            // Act
            var result = UILogics.NidCkecker(textBox);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void NidCkecker_WithInvalidNid_ReturnsFalse()
        {
            // Arrange
            var textBox = new TextBox { Text = "123abc456" };

            // Act
            var result = UILogics.NidCkecker(textBox);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void TryDetermineUiState_ReturnsTrue()
        {
            // Act
            var result = UILogics.TryDetermineUiState(out bool active);

            // Assert
            result.Should().BeTrue();
            active.Should().BeTrue();
        }
    }
}
