using System;
using System.Numerics;
using Veldrid;
using Veldrid.StartupUtilities;
using Veldrid.Sdl2;
using Veldrid.SPIRV;
using System.Collections.Generic;
using System.Threading;

namespace SplinterCamoForAlex
{
    class Program
    {
        static GShader s;
        static RandomVertexSet vertexSet;
        static Sdl2Window window;
        static bool updateFromThread1;
        static void Main(string[] args)
        {

            WindowCreateInfo windowCI = new WindowCreateInfo()
            {
                X = 100,
                Y = 100,
                WindowWidth = 960,
                WindowHeight = 540,
                WindowTitle = "Veldrid Tutorial"
            };
            window = VeldridStartup.CreateWindow(ref windowCI);

            s = new GShader(window);
            s.fillMode = PolygonFillMode.Solid;
            s.topo = PrimitiveTopology.PointList;

            List<VertexPositionColour> verts = new List<VertexPositionColour>()
            {
                new VertexPositionColour(new Vector2(0.9f,0.9f), new RgbaFloat(1,1,1,1)),
                new VertexPositionColour(new Vector2(-0.9f,0.9f), new RgbaFloat(1,1,1,1)),
                new VertexPositionColour(new Vector2(-0.9f,-0.9f), new RgbaFloat(1,1,1,1)),
                new VertexPositionColour(new Vector2(0.9f,-0.9f), new RgbaFloat(1,1,1,1))

            };

            //INITIALISE VERTEX SET HERE


            s.verteces = verts;
            s.GenerateIndeces();
            s.CreateResources();

            Thread vertexThread = new Thread(new ThreadStart(WaitForVertex));
            vertexThread.Start();

            while (window.Exists)
            {
                window.PumpEvents();
                if (updateFromThread1)
                {
                    AddVertex(/*INSERT FUNCTION TO GENERATE ONE RANDOM VERTEX HERE*/);
                    updateFromThread1 = false;
                }
                s.Draw();
            }
            s.DisposeResources();
        }

        static public void WaitForVertex()
        {
            while (window.Exists)
            {
                if (Console.ReadKey().Key == ConsoleKey.Spacebar)
                {
                    updateFromThread1 = true;
                }
            }
        }

        static public void AddVertex(VertexPositionColour vertex)
        {
            s.GenerateIndeces();
            s.verteces.Add(vertex);
            s.UpdateResources();
        }

    }
    struct VertexPositionColour
    {
        public Vector2 Position; // This is the position, in normalized device coordinates.
        public RgbaFloat Color; // This is the color of the vertex.
        public VertexPositionColour(Vector2 position, RgbaFloat color)
        {
            Position = position;
            Color = color;
        }
        public const uint SizeInBytes = 24;
    }

}
