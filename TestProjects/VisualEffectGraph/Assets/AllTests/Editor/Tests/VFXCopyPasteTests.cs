using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.VFX;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.VFX.UI;
using System.IO;
using UnityEditor.VFX.Block.Test;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine.Experimental.UIElements;

namespace UnityEditor.VFX.Test
{
    [TestFixture]
    public class VFXCopyPasteTests
    {
        VFXViewPresenter m_ViewPresenter;

        const string testAssetName = "Assets/TmpTests/VFXGraph1.asset";

        private int m_StartUndoGroupId;

        [SetUp]
        public void CreateTestAsset()
        {
            VFXAsset asset = new VFXAsset();

            var directoryPath = Path.GetDirectoryName(testAssetName);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            AssetDatabase.CreateAsset(asset, testAssetName);

            m_ViewPresenter = VFXViewPresenter.Manager.GetPresenter(asset);

            m_StartUndoGroupId = Undo.GetCurrentGroup();
        }

        [TearDown]
        public void DestroyTestAsset()
        {
            Undo.RevertAllDownToGroup(m_StartUndoGroupId);
            AssetDatabase.DeleteAsset(testAssetName);
        }

        [Test]
        public void CopyPasteContextWithBlock()
        {
            var initContextDesc = VFXLibrary.GetContexts().Where(t => t.name == "Initialize").First();

            var newContext = m_ViewPresenter.AddVFXContext(new Vector2(100, 100), initContextDesc);

            Assert.AreEqual(m_ViewPresenter.allChildren.Where(t => t is VFXContextPresenter).Count(), 1);

            var contextPresenter = m_ViewPresenter.allChildren.OfType<VFXContextPresenter>().First();

            Assert.AreEqual(contextPresenter.model, newContext);

            var flipBookBlockDesc = VFXLibrary.GetBlocks().First(t => t.name == "Flipbook Set TexIndex");

            contextPresenter.AddBlock(0, flipBookBlockDesc.CreateInstance());

            VFXViewWindow window = EditorWindow.GetWindow<VFXViewWindow>();

            window.presenter = m_ViewPresenter;

            VFXView view = window.graphView as VFXView;
            view.presenter = m_ViewPresenter;

            view.ClearSelection();
            foreach (var element in view.Query().OfType<GraphElement>().ToList().OfType<ISelectable>())
            {
                view.AddToSelection(element);
            }

            VFXSlot boundsSlot = newContext.GetInputSlot(0);

            AABox originalBounds = new AABox() { center = Vector3.one, size = Vector3.one * 10 };
            boundsSlot.value = originalBounds;

            VFXBlock flipBookBlock = m_ViewPresenter.elements.OfType<VFXContextPresenter>().First().blockPresenters.First().block;
            VFXSlot minValueSlot = flipBookBlock.GetInputSlot(0);


            float originalMinValue = 123.456f;
            minValueSlot.value = originalMinValue;

            view.CopySelectionCallback();

            boundsSlot.value = new AABox() { center = Vector3.zero, size = Vector3.zero };
            minValueSlot.value = 789f;

            view.PasteCallback();
            var elements = view.Query().OfType<GraphElement>().ToList();

            var contexts = elements.OfType<VFXContextUI>().ToArray();
            var copyContext = elements.OfType<VFXContextUI>().Select(t => t.GetPresenter<VFXContextPresenter>()).First(t => t.context != newContext).context;

            var copyBoundsSlot = copyContext.GetInputSlot(0);
            var copyMinSlot = copyContext[0].GetInputSlot(0);

            Assert.AreEqual((AABox)copyBoundsSlot.value, originalBounds);
            Assert.AreEqual((float)copyMinSlot.value, originalMinValue);
            Assert.AreNotEqual(copyContext.position, newContext.position);


            view.PasteCallback();

            elements = view.Query().OfType<GraphElement>().ToList();
            contexts = elements.OfType<VFXContextUI>().ToArray();

            var copy2Context = contexts.First(t => t.GetPresenter<VFXContextPresenter>().context != newContext && t.GetPresenter<VFXContextPresenter>().context != copyContext).GetPresenter<VFXContextPresenter>().context;

            Assert.AreNotEqual(copy2Context.position, newContext.position);
            Assert.AreNotEqual(copy2Context.position, copyContext.position);
        }

        [Test]
        public void CopyPasteOperator()
        {
            var crossOperatorDesc = VFXLibrary.GetOperators().Where(t => t.name == "Cross Product").First();

            var newOperator = m_ViewPresenter.AddVFXOperator(new Vector2(100, 100), crossOperatorDesc);

            var operatorPresenter = m_ViewPresenter.allChildren.OfType<VFXOperatorPresenter>().First();

            Assert.AreEqual(operatorPresenter.model, newOperator);

            VFXViewWindow window = EditorWindow.GetWindow<VFXViewWindow>();

            window.presenter = m_ViewPresenter;

            VFXView view = window.graphView as VFXView;
            view.presenter = m_ViewPresenter;

            view.ClearSelection();
            foreach (var element in view.Query().OfType<GraphElement>().ToList().OfType<ISelectable>())
            {
                view.AddToSelection(element);
            }


            VFXSlot aSlot = newOperator.GetInputSlot(0);

            Vector3 originalA = Vector3.one * 123;
            aSlot.value = originalA;

            view.CopySelectionCallback();

            aSlot.value = Vector3.one * 456;

            view.PasteCallback();

            var elements = view.Query().OfType<GraphElement>().ToList();

            var copyOperator = elements.OfType<VFXOperatorUI>().First(t => t.GetPresenter<VFXOperatorPresenter>().Operator != newOperator);

            var copaASlot = copyOperator.GetPresenter<VFXOperatorPresenter>().Operator.GetInputSlot(0);

            Assert.AreEqual((Vector3)copaASlot.value, originalA);

            Assert.AreNotEqual(copyOperator.GetPresenter<VFXOperatorPresenter>().Operator.position, newOperator.position);

            view.PasteCallback();

            elements = view.Query().OfType<GraphElement>().ToList();
            var copy2Operator = elements.OfType<VFXOperatorUI>().First(t => t.GetPresenter<VFXOperatorPresenter>().Operator != newOperator && t != copyOperator);

            Assert.AreNotEqual(copy2Operator.GetPresenter<VFXOperatorPresenter>().Operator.position, newOperator.position);
            Assert.AreNotEqual(copy2Operator.GetPresenter<VFXOperatorPresenter>().Operator.position, copyOperator.GetPresenter<VFXOperatorPresenter>().Operator.position);
        }

        [Test]
        public void CopyPasteEdges()
        {
            VFXAsset asset = AssetDatabase.LoadAssetAtPath<VFXAsset>("Assets/VFXEditor/Editor/Tests/CopyPasteTest.asset");

            VFXViewPresenter presenter = VFXViewPresenter.Manager.GetPresenter(asset);

            VFXViewWindow window = EditorWindow.GetWindow<VFXViewWindow>();
            VFXView view = window.graphView as VFXView;

            window.presenter = presenter;
            view.presenter = presenter;

            view.ClearSelection();


            foreach (var element in view.Query().OfType<GraphElement>().ToList().OfType<ISelectable>())
            {
                view.AddToSelection(element);
            }

            view.CopySelectionCallback();

            window.presenter = m_ViewPresenter;
            view.presenter = m_ViewPresenter;

            view.PasteCallback();

            VFXParameterUI[] parameters = view.Query().OfType<VFXParameterUI>().ToList().ToArray();

            Assert.AreEqual(parameters.Length, 2);

            if (parameters[0].title == "Vector3")
            {
                var tmp = parameters[0];
                parameters[0] = parameters[1];
                parameters[1] = tmp;
            }

            VFXOperatorUI[] operators = view.Query().OfType<VFXOperatorUI>().ToList().ToArray();

            Assert.AreEqual(operators.Length, 2);

            VFXContextUI[] contexts = view.Query().OfType<VFXContextUI>().ToList().ToArray();

            Assert.AreEqual(contexts.Length, 2);

            if (contexts[0].GetPresenter<VFXContextPresenter>().context is VFXBasicUpdate)
            {
                var tmp = contexts[0];
                contexts[0] = contexts[1];
                contexts[1] = tmp;
            }


            VFXDataEdge[] dataEdges = view.Query().OfType<VFXDataEdge>().ToList().ToArray();

            Assert.AreEqual(dataEdges.Length, 4);

            VFXOperator[] operatorModels = operators.Select(u => u.GetPresenter<VFXOperatorPresenter>().Operator).ToArray();

            Assert.IsNotNull(dataEdges.Where(t =>
                    t.output.GetFirstAncestorOfType<VFXSlotContainerUI>() == parameters[1] &&
                    operators.Contains(t.input.GetFirstAncestorOfType<VFXOperatorUI>())
                    ).FirstOrDefault());

            Assert.IsNotNull(dataEdges.Where(t =>
                    operators.Contains(t.input.GetFirstAncestorOfType<VFXOperatorUI>()) &&
                    operators.Contains(t.output.GetFirstAncestorOfType<VFXOperatorUI>()) &&
                    t.output.GetFirstAncestorOfType<VFXSlotContainerUI>() != t.input.GetFirstAncestorOfType<VFXSlotContainerUI>()
                    ).FirstOrDefault());

            Assert.IsNotNull(dataEdges.Where(t =>
                    t.output.GetFirstAncestorOfType<VFXSlotContainerUI>() == parameters[0] &&
                    t.input.GetFirstAncestorOfType<VFXSlotContainerUI>() == contexts[0].ownData
                    ).FirstOrDefault());

            Assert.IsNotNull(dataEdges.Where(t =>
                    operators.Contains(t.output.GetFirstAncestorOfType<VFXSlotContainerUI>()) &&
                    t.input.GetFirstAncestorOfType<VFXSlotContainerUI>() == contexts[0].GetAllBlocks().First()
                    ).FirstOrDefault());


            VFXFlowEdge flowEdge = view.Query().OfType<VFXFlowEdge>();

            Assert.IsNotNull(flowEdge);

            Assert.AreEqual(flowEdge.output.GetFirstAncestorOfType<VFXContextUI>(), contexts[1]);
            Assert.AreEqual(flowEdge.input.GetFirstAncestorOfType<VFXContextUI>(), contexts[0]);
        }
    }
}
