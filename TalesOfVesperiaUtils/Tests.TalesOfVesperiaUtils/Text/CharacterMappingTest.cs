using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TalesOfVesperiaUtils.Text;

namespace Tests.TalesOfVesperiaUtils.Text
{
	[TestClass]
	public class CharacterMappingTest
	{
		[TestMethod]
		public void TestMapping()
		{
			var Input = "¡Hola España! ¿Hola ESPAÑA? ñÑ¿¡";
			var ExpectedOutput = "ÏHola Espaãa! ËHola ESPAÃA? ãÃËÏ";
			Assert.AreEqual(ExpectedOutput, CharacterMapping.Instance.Map(Input));
			Assert.AreEqual(Input, CharacterMapping.Instance.Unmap(CharacterMapping.Instance.Map(Input)));
		}
	}
}