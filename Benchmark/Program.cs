using Benchmark;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

BenchmarkRunner.Run<BenchTest>();
//BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, new DebugInProcessConfig());