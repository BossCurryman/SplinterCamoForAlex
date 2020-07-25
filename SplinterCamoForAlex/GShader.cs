using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using Veldrid;
using Veldrid.StartupUtilities;
using Veldrid.Sdl2;
using Veldrid.SPIRV;

namespace SplinterCamoForAlex
{
    class GShader
    {
        public List<VertexPositionColour> verteces { get; set; } = new List<VertexPositionColour>();
        public ushort[] indeces { get; protected set; }
        public PolygonFillMode fillMode { get; set; }
        public PrimitiveTopology topo { get; set; }

        private GraphicsDevice graphicsDevice;
        private ResourceFactory factory;
        private CommandList commandList;
        private DeviceBuffer vertexBuffer;
        private DeviceBuffer indexBuffer;
        private static Shader[] shaders;
        private Pipeline pipeline;
        private const string VertexCode = @"
#version 450

layout(location = 0) in vec2 Position;
layout(location = 1) in vec4 Color;

layout(location = 0) out vec4 fsin_Color;

void main()
{
    gl_Position = vec4(Position, 0, 1);
    fsin_Color = Color;
}";

        private const string FragmentCode = @"
#version 450

layout(location = 0) in vec4 fsin_Color;
layout(location = 0) out vec4 fsout_Color;

void main()
{
    fsout_Color = fsin_Color;
}";

        public GShader(Sdl2Window window)
        {
            graphicsDevice = VeldridStartup.CreateGraphicsDevice(window);
            factory = graphicsDevice.ResourceFactory;
        }

        public void GenerateIndeces()
        {
            ushort[] g = new ushort[verteces.Count];
            for (int i = 0; i < g.Length; ++i)
            {
                g[i] = (ushort)i;
            }
            indeces = g;
        }

        public void UpdateIndeces()
        {

        }

        public void Draw()
        {
            commandList.Begin();
            commandList.SetFramebuffer(graphicsDevice.SwapchainFramebuffer);
            commandList.ClearColorTarget(0, RgbaFloat.Black);

            commandList.SetVertexBuffer(0, vertexBuffer);
            commandList.SetIndexBuffer(indexBuffer, IndexFormat.UInt16);
            commandList.SetPipeline(pipeline);
            commandList.DrawIndexed(
                indexCount: (uint)indeces.Length,
                instanceCount: 1,
                indexStart: 0,
                vertexOffset: 0,
                instanceStart: 0);
            commandList.End();
            graphicsDevice.SubmitCommands(commandList);
            graphicsDevice.SwapBuffers();
        }

        public void DisposeResources()
        {
            pipeline.Dispose();
            shaders[0].Dispose();
            shaders[1].Dispose();
            commandList.Dispose();
            vertexBuffer.Dispose();
            indexBuffer.Dispose();
            graphicsDevice.Dispose();
        }

        public void CreateResources()
        {

            /*
            VertexPositionColor[] quadVertices =
            {
                 new VertexPositionColor(new Vector2(-.75f, .75f), RgbaFloat.Red),
                 new VertexPositionColor(new Vector2(.75f, .75f), RgbaFloat.Green),
                 new VertexPositionColor(new Vector2(-.75f, -.75f), RgbaFloat.Blue),
                 new VertexPositionColor(new Vector2(.75f, -.75f), RgbaFloat.Yellow)
            };

            ushort[] quadIndices = { 0, 1, 2, 3 };
            */
            vertexBuffer = factory.CreateBuffer(new BufferDescription((uint)verteces.Count * VertexPositionColour.SizeInBytes, BufferUsage.VertexBuffer));
            indexBuffer = factory.CreateBuffer(new BufferDescription((uint)indeces.Length * sizeof(ushort), BufferUsage.IndexBuffer));

            graphicsDevice.UpdateBuffer(vertexBuffer, 0, verteces.ToArray());
            graphicsDevice.UpdateBuffer(indexBuffer, 0, indeces);

            VertexLayoutDescription vertexLayout = new VertexLayoutDescription(
                new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4));


            ShaderDescription vertexShaderDesc = new ShaderDescription(
                ShaderStages.Vertex,
                Encoding.UTF8.GetBytes(VertexCode),
                "main");
            ShaderDescription fragmentShaderDesc = new ShaderDescription(
                ShaderStages.Fragment,
                Encoding.UTF8.GetBytes(FragmentCode),
                "main");

            shaders = factory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);

            GraphicsPipelineDescription pipelineDescription = new GraphicsPipelineDescription();
            pipelineDescription.BlendState = BlendStateDescription.SingleOverrideBlend;

            pipelineDescription.DepthStencilState = new DepthStencilStateDescription(
                depthTestEnabled: true,
                depthWriteEnabled: true,
                comparisonKind: ComparisonKind.LessEqual);

            pipelineDescription.RasterizerState = new RasterizerStateDescription(
                cullMode: FaceCullMode.Back,
                fillMode: fillMode,
                frontFace: FrontFace.Clockwise,
                depthClipEnabled: true,
                scissorTestEnabled: false);

            pipelineDescription.PrimitiveTopology = topo;
            pipelineDescription.ResourceLayouts = Array.Empty<ResourceLayout>();
            pipelineDescription.ShaderSet = new ShaderSetDescription(
                vertexLayouts: new VertexLayoutDescription[] { vertexLayout },
                shaders: shaders);
            pipelineDescription.Outputs = graphicsDevice.SwapchainFramebuffer.OutputDescription;


            pipeline = factory.CreateGraphicsPipeline(pipelineDescription);

            commandList = factory.CreateCommandList();
        }

        public void UpdateResources()
        {
            vertexBuffer = factory.CreateBuffer(new BufferDescription((uint)verteces.Count * VertexPositionColour.SizeInBytes, BufferUsage.VertexBuffer));
            indexBuffer = factory.CreateBuffer(new BufferDescription((uint)indeces.Length * sizeof(ushort), BufferUsage.IndexBuffer));

            graphicsDevice.UpdateBuffer(vertexBuffer, 0, verteces.ToArray());
            graphicsDevice.UpdateBuffer(indexBuffer, 0, indeces);
        }

    }
}
