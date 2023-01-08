using System.Collections;
using System.Collections.Generic;
using GamificationPlayer.DTO.AnnounceDeviceFlow;
using NUnit.Framework;
using UnityEngine;

namespace GamificationPlayer.Tests
{
    public class AnnounceDeviceFlowRequestDTOTest
    {
        [Test]
        public void TestConstructor()
        {
            var obj = new AnnounceDeviceFlowRequestDTO();

            Assert.IsNotNull(obj.data);

            Assert.AreEqual("device_login", obj.data.Type);
        }
    }
}
