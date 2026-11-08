using System.Collections.Generic;
using System.Linq;
using CardRhythm.Procedure;
using GameFramework.Procedure;
using NUnit.Framework;

namespace CardRhythm.Tests.EditMode
{
    /// <summary>
    /// 流程清单的回归测试：防止加了新流程却忘了注册，或入口流程被改坏。
    /// </summary>
    public class ProcedureTests
    {
        [Test]
        public void EveryProcedureIsRegisteredInCatalog()
        {
            var procedures = typeof(GameProcedureBase).Assembly
                .GetTypes()
                .Where(t => !t.IsAbstract && typeof(GameProcedureBase).IsAssignableFrom(t))
                .ToList();

            Assert.IsNotEmpty(procedures, "没有找到任何流程类");
            foreach (var procedure in procedures)
            {
                CollectionAssert.Contains(ProcedureCatalog.AvailableTypeNames, procedure.FullName,
                    "流程没有注册到 ProcedureCatalog：" + procedure.Name);
            }
        }

        [Test]
        public void Catalog_HasNoDuplicates()
        {
            var names = new HashSet<string>();
            foreach (string name in ProcedureCatalog.AvailableTypeNames)
            {
                Assert.IsTrue(names.Add(name), "流程重复注册：" + name);
            }
        }

        [Test]
        public void EntranceProcedure_IsLaunch()
        {
            Assert.AreEqual(typeof(ProcedureLaunch).FullName, ProcedureCatalog.EntranceTypeName);
            CollectionAssert.Contains(ProcedureCatalog.AvailableTypeNames, ProcedureCatalog.EntranceTypeName);
        }

        [Test]
        public void EveryProcedureDerivesFromGfProcedureBase()
        {
            foreach (string name in ProcedureCatalog.AvailableTypeNames)
            {
                var type = typeof(GameProcedureBase).Assembly.GetType(name);
                Assert.IsNotNull(type, "找不到流程类型：" + name);
                Assert.IsTrue(typeof(ProcedureBase).IsAssignableFrom(type), "流程必须派生自 ProcedureBase：" + name);
            }
        }
    }
}
