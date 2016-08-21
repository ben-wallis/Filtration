using System;
using Filtration.Models;
using Filtration.Services;
using Moq;
using NUnit.Framework;

namespace Filtration.Tests.Services
{
    [TestFixture]
    public class TestUpdateService
    {
//        [Test]
//        public void DeserializeUpdateData_ReturnsCorrectData()
//        {
//            // Arrange
//            var testInputData = @"<UpdateData>
//	                                <LatestVersionMajorPart>1</LatestVersionMajorPart>
//                                    <LatestVersionMinorPart>3</LatestVersionMinorPart>
//	                                <ReleaseDate>2015-07-01</ReleaseDate>
//                                    <DownloadUrl>http://www.google.com</DownloadUrl>
//	                                <ReleaseNotes>* Release notes line 1
//* Release notes line 2
//* More really great release notes!</ReleaseNotes>
//                                </UpdateData>";

//            var expectedResult = new UpdateData
//            {
//                LatestVersionMajorPart = 1,
//                LatestVersionMinorPart = 3,
//                DownloadUrl = "http://www.google.com",
//                ReleaseDate = new DateTime(2015, 7, 1),
//                ReleaseNotes = @"* Release notes line 1
//* Release notes line 2
//* More really great release notes!"
//            };

//            var mockHTTPService = new Mock<IHTTPService>();
//            var service = new UpdateCheckService(mockHTTPService.Object);

//            // Act
//            var result = service.DeserializeUpdateData(testInputData);

//            // Assert
//            Assert.AreEqual(expectedResult.LatestVersionMajorPart, result.LatestVersionMajorPart);
//            Assert.AreEqual(expectedResult.LatestVersionMinorPart, result.LatestVersionMinorPart);
//            Assert.AreEqual(expectedResult.DownloadUrl, result.DownloadUrl);
//            Assert.AreEqual(expectedResult.ReleaseDate, result.ReleaseDate);
//            Assert.AreEqual(expectedResult.ReleaseNotes, result.ReleaseNotes);
//        }

    }
}
