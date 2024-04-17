using Python.Runtime;

var script = await File.ReadAllTextAsync(@"D:\6SEM\MTran\MTran\Python.Interpreter\test1.py");

RunScript(script);

static void RunScript(string script)
{
	PythonEngine.PythonPath = @"C:\Users\zahar\AppData\Local\Programs\Python\Python310\Lib";
	PythonEngine.PythonHome = @"C:\Users\zahar\AppData\Local\Programs\Python\Python310";

	//Runtime.PythonDLL = @"C:\Users\zahar\AppData\Local\Programs\Python\Python310\pythong310.dll";
	PythonEngine.Initialize();

	using (Py.GIL())
	{
		using var scope = Py.CreateScope();
		scope.Exec(script);
	}
}
