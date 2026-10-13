# Coding style references

The following lecturer materials were available locally and used as references. They are not redistributed in this repository.

| Material | Applied here |
| --- | --- |
| Lecture 5 - FileHandler, slides 13-18 | Ordinary class properties, a constructor, ToString, separate file-handling methods and a List return value |
| Lecture3 - Reading TextFiles | File.ReadAllLines and reading text from a file |
| Lecture2 - Writing to Text Files | File.WriteAllLines and closing the file after writing |
| Lecture4 - Collections | List<Animal>, arrays, for and foreach loops |

The application keeps explicit using statements and types, block namespaces, ordinary if/else conditions and named WinForms event methods. It has one FileHandler rather than an interface and separate practice/production repositories.

Two project-specific choices extend the basic examples:

- The file path is based on the executable directory instead of a hardcoded user folder, as required by the project.
- Updates write a temporary file and then call File.Replace so a failed write does not truncate the original. Comments explain this extra step.

The forms, helper names and validation messages are tailored to this project; this is not a verbatim copy of the lecturer's implementation. The shared model and rules should be reviewed with Member 1, and the loading/error support with Member 2.
