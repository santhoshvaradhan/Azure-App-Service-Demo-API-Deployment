using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;

namespace Azure_App_Service_Demo_API_Deployment.Controllers
{
    public class Student
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Course { get; set; } = string.Empty;
    }

    [ApiController]
    [Route("api/[controller]")]
    public class StudentsController : ControllerBase
    {
        // Thread-safe in-memory store seeded with default values
        private static readonly ConcurrentDictionary<int, Student> Students = new(
            new[]
            {
                new Student { Id = 1, Name = "John Doe", Age = 20, Course = "Computer Science" },
                new Student { Id = 2, Name = "Jane Smith", Age = 22, Course = "Mathematics" },
                new Student { Id = 3, Name = "Alex Johnson", Age = 21, Course = "Physics" }
            }.ToDictionary(s => s.Id));

        private static int _nextId = 3;

        [HttpGet]
        public ActionResult<IEnumerable<Student>> GetAll() =>
            Ok(Students.Values.OrderBy(s => s.Id));

        [HttpGet("{id:int}")]
        public ActionResult<Student> GetById(int id) =>
            Students.TryGetValue(id, out var student) ? Ok(student) : NotFound();

        [HttpPost]
        public ActionResult<Student> Create(Student student)
        {
            student.Id = Interlocked.Increment(ref _nextId);
            Students[student.Id] = student;
            return CreatedAtAction(nameof(GetById), new { id = student.Id }, student);
        }

        [HttpPut("{id:int}")]
        public IActionResult Update(int id, Student student)
        {
            if (!Students.ContainsKey(id))
                return NotFound();

            student.Id = id;
            Students[id] = student;
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id) =>
            Students.TryRemove(id, out _) ? NoContent() : NotFound();
    }
}
