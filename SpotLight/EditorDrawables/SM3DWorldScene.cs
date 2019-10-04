﻿using BYAML;
using GL_EditorFramework.EditorDrawables;
using GL_EditorFramework.GL_Core;
using GL_EditorFramework.Interfaces;
using OpenTK.Graphics.OpenGL;
using Syroot.BinaryData;
using Syroot.Maths;
using Syroot.NintenTools.Bfres;
using Syroot.NintenTools.Bfres.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SZS;

namespace SpotLight.EditorDrawables
{
    class SM3DWorldScene : CategorizedScene
    {
        /// <summary>
        /// Filepath to the ObjectData Folder. <para/>Example: ObjectData + "BlockPow.szs"
        /// </summary>
        readonly string ObjectData = "D:/Dismanteled/WiiU/SM3DW Base/content/ObjectData/"; //This should be gained from the filepath of the Level
        //C:/Users/Jupahe/Documents/3dworldstuff/ObjectData/
        //Left ^ here just in case
        BfresModelCache.CachedModel testModel;
        BfresModelCache.CachedModel testModel2;

        public SM3DWorldScene()
        {
            
        }

        public override void Prepare(GL_ControlModern control)
        {
            BfresModelCache.Initialize(control);

            SarcData objArc = SARC.UnpackRamN(YAZ0.Decompress(ObjectData + "BlockQuestion.szs"));

            testModel = new BfresModelCache.CachedModel(new MemoryStream(objArc.Files["BlockQuestion.bfres"]), control);

            objArc = SARC.UnpackRamN(YAZ0.Decompress(ObjectData + "BlockBrick.szs"));

            testModel2 = new BfresModelCache.CachedModel(new MemoryStream(objArc.Files["BlockBrick.bfres"]), control);

            base.Prepare(control);
        }

        public override void Draw(GL_ControlModern control, Pass pass)
        {
            control.ResetModelMatrix();

            for(int i = 0; i<100; i++)
            {
                testModel.Draw(control);
                control.ApplyModelTransform(OpenTK.Matrix4.CreateTranslation(1, 0, 0));
                testModel2.Draw(control);
                control.ApplyModelTransform(OpenTK.Matrix4.CreateTranslation(1, 0, 0));
            }

            base.Draw(control, pass);
        }

        public List<I3dWorldObject> linkedObjects = new List<I3dWorldObject>();

        protected override IEnumerable<IEditableObject> GetObjects()
        {
            foreach (List<IEditableObject> objects in categories.Values)
            {
                foreach (IEditableObject obj in objects)
                    yield return obj;
            }

            foreach (I3dWorldObject obj in linkedObjects)
                yield return obj;
        }

        public override void DeleteSelected()
        {
            DeletionManager manager = new DeletionManager();

            foreach (List<IEditableObject> objects in categories.Values)
            {
                foreach (IEditableObject obj in objects)
                    obj.DeleteSelected(manager, objects, CurrentList);
            }

            foreach (I3dWorldObject obj in linkedObjects)
                obj.DeleteSelected(manager, linkedObjects, CurrentList);

            _ExecuteDeletion(manager);
        }

        public void Save(ByamlNodeWriter writer, string levelName, string categoryName)
        {
            ByamlNodeWriter.DictionaryNode rootNode = writer.CreateDictionaryNode(categories);

            ByamlNodeWriter.ArrayNode objsNode = writer.CreateArrayNode();

            HashSet<I3dWorldObject> alreadyWrittenObjs = new HashSet<I3dWorldObject>();
            
            rootNode.AddDynamicValue("FilePath", $"D:/home/TokyoProject/RedCarpet/Asset/StageData/{levelName}/Map/{levelName}{categoryName}.muunt");

            foreach (KeyValuePair<string, List<IEditableObject>> keyValuePair in categories)
            {
                ByamlNodeWriter.ArrayNode categoryNode = writer.CreateArrayNode(keyValuePair.Value);

                foreach (I3dWorldObject obj in keyValuePair.Value)
                {
                    if (!alreadyWrittenObjs.Contains(obj))
                    {
                        ByamlNodeWriter.DictionaryNode objNode = writer.CreateDictionaryNode(obj);
                        obj.Save(alreadyWrittenObjs, writer, objNode, false);
                        categoryNode.AddDictionaryNodeRef(objNode);
                        objsNode.AddDictionaryNodeRef(objNode);
                    }
                    else
                    {
                        categoryNode.AddDictionaryRef(obj);
                        objsNode.AddDictionaryRef(obj);
                    }
                }
                rootNode.AddArrayNodeRef(keyValuePair.Key, categoryNode, true);
            }

            rootNode.AddArrayNodeRef("Objs", objsNode);

            writer.Write(rootNode, true);
        }
    }
}
