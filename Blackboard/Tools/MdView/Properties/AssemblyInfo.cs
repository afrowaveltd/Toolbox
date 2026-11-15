using System.Runtime.CompilerServices;

// Grant regression tests access to implementation-only prompt helpers
// without making the live-render hook part of the public MdView API.
[assembly: InternalsVisibleTo("MdView.Tests")]
