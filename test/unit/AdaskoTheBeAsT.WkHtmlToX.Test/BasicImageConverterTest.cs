using System;
using System.IO;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.Documents;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using Xunit;

namespace AdaskoTheBeAsT.WkHtmlToX.Test
{
    public sealed partial class BasicImageConverterTest
        : IDisposable
    {
        private readonly Fixture _fixture;
        private readonly Mock<IWkHtmlToXModule> _module;
        private readonly BasicImageConverter _sut;

        public BasicImageConverterTest()
        {
            _fixture = new Fixture();

            var moduleFactoryMock = new Mock<IWkHtmlToXModuleFactory>();
            _module = new Mock<IWkHtmlToXModule>();
            _module.Setup(m => m.Dispose());
            moduleFactoryMock.Setup(mf => mf.GetModule(It.IsAny<ModuleKind>()))
                .Returns(_module.Object);

            _sut = new BasicImageConverter(moduleFactoryMock.Object);
        }

        public void Dispose()
        {
            _sut.Dispose();
        }

        [Fact]
        public void ShouldNotThrowWhenUsingDefaultConstructor()
        {
            // Arrange
            // ReSharper disable once AssignmentIsFullyDiscarded
            Action action = () => _ = new BasicImageConverter();

            // Act & Assert
            action.Should().NotThrow();
        }

        [Fact]
        public void CreateConverterShouldThrowArgumentNullExceptionWhenNullPassed()
        {
            // Arrange
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

            // ReSharper disable once AssignmentIsFullyDiscarded
            Action action = () => _ = _sut.CreateConverter(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            // Act & Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void CreateConverterShouldInvokeCreateGlobalSettings()
        {
            // Arrange
            var globalSettingsPtr = new IntPtr(_fixture.Create<int>());
            var converterPtr = new IntPtr(_fixture.Create<int>());
            _module.Setup(m =>
                    m.CreateGlobalSettings())
                .Returns(globalSettingsPtr);
            _module.Setup(m =>
                    m.CreateConverter(It.IsAny<IntPtr>()))
                .Returns(converterPtr);
            _module.Setup(
                m =>
                    m.SetGlobalSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()));
            var document = new HtmlToImageDocument();

            // Act
            var result = _sut.CreateConverter(document);

            // Assert
            using (new AssertionScope())
            {
                _module.Verify(m => m.CreateGlobalSettings(), Times.Once);
                _module.Verify(
                    m =>
                        m.SetGlobalSetting(
                            It.IsAny<IntPtr>(),
                            It.IsAny<string>(),
                            It.IsAny<string?>()),
                    Times.Never);
                result.converterPtr.Should().Be(converterPtr);
                result.globalSettingsPtr.Should().Be(globalSettingsPtr);
            }
        }

        [Fact]
        public void CreateConverterShouldSetGlobalSettings()
        {
            // Arrange
            var globalSettingsPtr = new IntPtr(_fixture.Create<int>());
            var converterPtr = new IntPtr(_fixture.Create<int>());
            _module.Setup(m =>
                    m.CreateGlobalSettings())
                .Returns(globalSettingsPtr);
            _module.Setup(m =>
                    m.CreateConverter(It.IsAny<IntPtr>()))
                .Returns(converterPtr);
            _module.Setup(
                m =>
                    m.SetGlobalSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()));
            var document = new HtmlToImageDocument();
            var quality = _fixture.Create<string>();
            document.ImageSettings.Quality = quality;

            // Act
            var result = _sut.CreateConverter(document);

            // Assert
            using (new AssertionScope())
            {
                _module.Verify(m => m.CreateGlobalSettings(), Times.Once);
                _module.Verify(
                    m =>
                        m.SetGlobalSetting(
                            It.Is<IntPtr>(v => v == globalSettingsPtr),
                            It.Is<string>(v => v == "quality"),
                            It.Is<string?>(v => v == quality)),
                    Times.Once);
                result.converterPtr.Should().Be(converterPtr);
                result.globalSettingsPtr.Should().Be(globalSettingsPtr);
            }
        }

        [Fact]
        public void ConvertImplShouldThrowExceptionWhenNullDocumentPassed()
        {
            // Arrange
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Action action = () => _sut.ConvertImpl(null, _ => Stream.Null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            // Act & Assert
            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ConvertImplShouldThrowExceptionWhenNullImageSettingsPassed()
        {
            // Arrange
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            var document = new HtmlToImageDocument
            {
                ImageSettings = null,
            };
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            Action action = () => _sut.ConvertImpl(document, _ => Stream.Null);

            // Act & Assert
            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ConvertImplShouldThrowExceptionWhenNullCreateStreamFuncPassed()
        {
            // Arrange
            var document = new HtmlToImageDocument();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Action action = () => _sut.ConvertImpl(document, null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            // Act & Assert
            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ConvertImplShouldThrowExceptionWhenModuleInitializeNotEqualOne()
        {
            // Arrange
            var document = new HtmlToImageDocument();
            _module.Setup(m => m.Initialize(It.IsAny<int>()))
                .Returns(0);

            Action action = () => _sut.ConvertImpl(document, _ => Stream.Null);

            // Act & Assert
            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ConvertImplShouldReturnNullStreamWhenNotConverted()
        {
            // Arrange
            var globalSettingsPtr = new IntPtr(_fixture.Create<int>());
            var converterPtr = new IntPtr(_fixture.Create<int>());
            _module.Setup(m => m.Initialize(It.IsAny<int>()))
                .Returns(1);
            _module.Setup(m =>
                    m.CreateGlobalSettings())
                .Returns(globalSettingsPtr);
            _module.Setup(m =>
                    m.CreateConverter(It.IsAny<IntPtr>()))
                .Returns(converterPtr);
            _module.Setup(
                m =>
                    m.SetGlobalSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()));
            _module.Setup(m => m.Convert(It.IsAny<IntPtr>()))
                .Returns(false);
            _module.Setup(m => m.GetOutput(It.IsAny<IntPtr>(), It.IsAny<Func<int, Stream>>()));
            _module.Setup(m => m.DestroyGlobalSetting(It.IsAny<IntPtr>()));
            _module.Setup(m => m.DestroyConverter(It.IsAny<IntPtr>()));
            _module.Setup(m => m.Terminate());
            var document = new HtmlToImageDocument();
            var quality = _fixture.Create<string>();
            document.ImageSettings.Quality = quality;

            // Act
            var result = _sut.ConvertImpl(document, _ => Stream.Null);

            // Assert
            using (new AssertionScope())
            {
                _module.Verify(m => m.Initialize(It.IsAny<int>()), Times.Once);
                _module.Verify(m => m.CreateGlobalSettings(), Times.Once);
                _module.Verify(
                    m =>
                        m.SetGlobalSetting(
                            It.Is<IntPtr>(v => v == globalSettingsPtr),
                            It.Is<string>(v => v == "quality"),
                            It.Is<string?>(v => v == quality)),
                    Times.Once);
                _module.Verify(m => m.GetOutput(It.IsAny<IntPtr>(), It.IsAny<Func<int, Stream>>()), Times.Never);
                _module.Verify(m => m.DestroyGlobalSetting(It.IsAny<IntPtr>()), Times.Once);
                _module.Verify(m => m.DestroyConverter(It.IsAny<IntPtr>()), Times.Once);
                _module.Verify(m => m.Terminate(), Times.Once);
                result.Should().BeFalse();
            }
        }

        [Fact]
        public void ConvertImplShouldReturnStreamWhenHtmlContentPassedAndConverted()
        {
            // Arrange
            using var memoryStream = new MemoryStream();
            var globalSettingsPtr = new IntPtr(_fixture.Create<int>());
            var converterPtr = new IntPtr(_fixture.Create<int>());
            _module.Setup(m => m.Initialize(It.IsAny<int>()))
                .Returns(1);
            _module.Setup(m =>
                    m.CreateGlobalSettings())
                .Returns(globalSettingsPtr);
            _module.Setup(m =>
                    m.CreateConverter(It.IsAny<IntPtr>()))
                .Returns(converterPtr);
            _module.Setup(
                m =>
                    m.SetGlobalSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()));
            _module.Setup(m => m.Convert(It.IsAny<IntPtr>()))
                .Returns(true);
            _module.Setup(m => m.GetOutput(It.IsAny<IntPtr>(), It.IsAny<Func<int, Stream>>()));
            _module.Setup(m => m.DestroyGlobalSetting(It.IsAny<IntPtr>()));
            _module.Setup(m => m.DestroyConverter(It.IsAny<IntPtr>()));
            _module.Setup(m => m.Terminate());
            var document = new HtmlToImageDocument();
            var quality = _fixture.Create<string>();
            document.ImageSettings.Quality = quality;

            // Act
            // ReSharper disable once AccessToDisposedClosure
            var result = _sut.ConvertImpl(document, _ => memoryStream);

            // Assert
            using (new AssertionScope())
            {
                _module.Verify(m => m.Initialize(It.IsAny<int>()), Times.Once);
                _module.Verify(m => m.CreateGlobalSettings(), Times.Once);
                _module.Verify(
                    m =>
                        m.SetGlobalSetting(
                            It.Is<IntPtr>(v => v == globalSettingsPtr),
                            It.Is<string>(v => v == "quality"),
                            It.Is<string?>(v => v == quality)),
                    Times.Once);
                _module.Verify(m => m.GetOutput(It.IsAny<IntPtr>(), It.IsAny<Func<int, Stream>>()), Times.Once);
                _module.Verify(m => m.DestroyGlobalSetting(It.IsAny<IntPtr>()), Times.Once);
                _module.Verify(m => m.DestroyConverter(It.IsAny<IntPtr>()), Times.Once);
                _module.Verify(m => m.Terminate(), Times.Once);
                result.Should().BeTrue();
            }
        }

        [Fact]
        public void ConvertShouldThrowExceptionWhenNullDocumentPassed()
        {
            // Arrange
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Action action = () => _sut.Convert(null, _ => Stream.Null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            // Act & Assert
            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ConvertShouldThrowExceptionWhenNullCreateStreamFuncPassed()
        {
            // Arrange
            var document = new HtmlToImageDocument();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Action action = () => _sut.Convert(document, null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            // Act & Assert
            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ConvertShouldReturnNullStreamWhenNotConverted()
        {
            // Arrange
            var globalSettingsPtr = new IntPtr(_fixture.Create<int>());
            var converterPtr = new IntPtr(_fixture.Create<int>());
            _module.Setup(m => m.Initialize(It.IsAny<int>()))
                .Returns(1);
            _module.Setup(m =>
                    m.CreateGlobalSettings())
                .Returns(globalSettingsPtr);
            _module.Setup(m =>
                    m.CreateConverter(It.IsAny<IntPtr>()))
                .Returns(converterPtr);
            _module.Setup(
                m =>
                    m.SetGlobalSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()));
            _module.Setup(m => m.Convert(It.IsAny<IntPtr>()))
                .Returns(false);
            _module.Setup(m => m.GetOutput(It.IsAny<IntPtr>(), It.IsAny<Func<int, Stream>>()));
            _module.Setup(m => m.DestroyGlobalSetting(It.IsAny<IntPtr>()));
            _module.Setup(m => m.DestroyConverter(It.IsAny<IntPtr>()));
            _module.Setup(m => m.Terminate());
            var document = new HtmlToImageDocument();
            var quality = _fixture.Create<string>();
            document.ImageSettings.Quality = quality;

            // Act
            var result = _sut.Convert(document, _ => Stream.Null);

            // Assert
            using (new AssertionScope())
            {
                _module.Verify(m => m.Initialize(It.IsAny<int>()), Times.Once);
                _module.Verify(m => m.CreateGlobalSettings(), Times.Once);
                _module.Verify(
                    m =>
                        m.SetGlobalSetting(
                            It.Is<IntPtr>(v => v == globalSettingsPtr),
                            It.Is<string>(v => v == "quality"),
                            It.Is<string?>(v => v == quality)),
                    Times.Once);
                _module.Verify(m => m.GetOutput(It.IsAny<IntPtr>(), It.IsAny<Func<int, Stream>>()), Times.Never);
                _module.Verify(m => m.DestroyGlobalSetting(It.IsAny<IntPtr>()), Times.Once);
                _module.Verify(m => m.DestroyConverter(It.IsAny<IntPtr>()), Times.Once);
                _module.Verify(m => m.Terminate(), Times.Once);
                result.Should().BeFalse();
            }
        }

        [Fact]
        public void ConvertShouldReturnStreamWhenConverted()
        {
            // Arrange
            using var memoryStream = new MemoryStream();
            var globalSettingsPtr = new IntPtr(_fixture.Create<int>());
            var converterPtr = new IntPtr(_fixture.Create<int>());
            _module.Setup(m => m.Initialize(It.IsAny<int>()))
                .Returns(1);
            _module.Setup(m =>
                    m.CreateGlobalSettings())
                .Returns(globalSettingsPtr);
            _module.Setup(m =>
                    m.CreateConverter(It.IsAny<IntPtr>()))
                .Returns(converterPtr);
            _module.Setup(
                m =>
                    m.SetGlobalSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()));
            _module.Setup(m => m.Convert(It.IsAny<IntPtr>()))
                .Returns(true);
            _module.Setup(m => m.GetOutput(It.IsAny<IntPtr>(), It.IsAny<Func<int, Stream>>()));
            _module.Setup(m => m.DestroyGlobalSetting(It.IsAny<IntPtr>()));
            _module.Setup(m => m.DestroyConverter(It.IsAny<IntPtr>()));
            _module.Setup(m => m.Terminate());
            var document = new HtmlToImageDocument();
            var quality = _fixture.Create<string>();
            document.ImageSettings.Quality = quality;

            // Act
            // ReSharper disable once AccessToDisposedClosure
            var result = _sut.Convert(document, _ => memoryStream);

            // Assert
            using (new AssertionScope())
            {
                _module.Verify(m => m.Initialize(It.IsAny<int>()), Times.Once);
                _module.Verify(m => m.CreateGlobalSettings(), Times.Once);
                _module.Verify(
                    m =>
                        m.SetGlobalSetting(
                            It.Is<IntPtr>(v => v == globalSettingsPtr),
                            It.Is<string>(v => v == "quality"),
                            It.Is<string?>(v => v == quality)),
                    Times.Once);
                _module.Verify(m => m.GetOutput(It.IsAny<IntPtr>(), It.IsAny<Func<int, Stream>>()), Times.Once);
                _module.Verify(m => m.DestroyGlobalSetting(It.IsAny<IntPtr>()), Times.Once);
                _module.Verify(m => m.DestroyConverter(It.IsAny<IntPtr>()), Times.Once);
                _module.Verify(m => m.Terminate(), Times.Once);
                result.Should().BeTrue();
            }
        }
    }
}
