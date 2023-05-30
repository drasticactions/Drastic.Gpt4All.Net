// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace Gpt4All.LibraryLoader;

public interface ILibraryLoader
{
    LoadResult OpenLibrary(string? fileName);
}
