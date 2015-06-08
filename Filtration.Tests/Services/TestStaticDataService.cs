using Filtration.Services;
using Moq;
using NUnit.Framework;

namespace Filtration.Tests.Services
{
    [TestFixture]
    public class TestStaticDataService
    {
        [Test]
        public void Constructor_CallsFileSystemService()
        {
            // Arrange

            var mockFileSystemService = new Mock<IFileSystemService>();
            mockFileSystemService.Setup(f => f.ReadFileAsString(It.IsAny<string>())).Returns("TestResult").Verifiable();

            var service = new StaticDataService(mockFileSystemService.Object);

            // Act
            
            // Assert
            mockFileSystemService.Verify();
        }

        [Ignore("Integration Test")]
        [Test]
        public void Constructor_ReadsFromFileCorrectly()
        {
            // Arrange

            var fileSystemService = new FileSystemService();

            var service = new StaticDataService(fileSystemService);

            // Act

            // Assert
            
        }

    }
}
