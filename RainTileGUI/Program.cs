using System.Drawing;
using Silk.NET.Windowing;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using ImGuiNET;

class Program {
    static void Main(string[] args) {
        using var window = Window.Create(WindowOptions.Default);

        ImGuiController controller = null;
        GL gl = null;
        IInputContext inputContext = null;

        window.Load += () => {
            controller = new ImGuiController(
                gl = window.CreateOpenGL(),
                window,
                inputContext = window.CreateInput()
            );
        };

        window.FramebufferResize += s => {
            gl.Viewport(s);
        };

        window.Render += delta => {
            controller.Update((float) delta);

            gl.ClearColor(Color.FromArgb(255, (int) (.45f * 255), (int) (.55f * 255), (int) (.60f * 255)));
            gl.Clear((uint) ClearBufferMask.ColorBufferBit);

            var io = ImGui.GetIO();
            io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
            ImGui.DockSpaceOverViewport();

            ImGuiNET.ImGui.ShowDemoWindow();

            controller.Render();
        };

        window.Closing += () => {
            controller?.Dispose();
            inputContext?.Dispose();
            gl?.Dispose();
        };

        window.Run();

        window.Dispose();
    }
}