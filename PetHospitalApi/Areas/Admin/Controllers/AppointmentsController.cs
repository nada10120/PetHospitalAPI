using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.DTOs.Request;
using Models.DTOs.Response;
using Repositories.IRepository;

namespace PetHospitalApi.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentRepository _appointmentRepository;

        public AppointmentsController(IAppointmentRepository appointmentRepository)
        {
            _appointmentRepository = appointmentRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAppointments()
        {
            var appointments = await _appointmentRepository.GetAsync();
            if (appointments == null)
            {
                return NotFound("No appointments found.");
            }
            return Ok(appointments.Adapt<IEnumerable<AppointmentResponse>>());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAppointmentById([FromRoute] int id)
        {
            var appointment = await _appointmentRepository.GetOneAsync(e => e.AppointmentId == id);
            if (appointment == null)
            {
                return NotFound($"Appointment with ID {id} not found.");
            }
            return Ok(appointment.Adapt<AppointmentResponse>());
        }
        [HttpPost]
        public async Task<IActionResult> CreateAppointment([FromBody] AppointmentRequest appointmentRequest)
        {
            if (appointmentRequest == null)
                return BadRequest("Appointment data is null.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var appointment = new Appointment
            {
                UserId = appointmentRequest.UserId,
                PetId = appointmentRequest.PetId,
                VetId = appointmentRequest.VetId,
                DateTime = appointmentRequest.DateTime ?? DateTime.UtcNow,
                Status = appointmentRequest.Status
            };

            var result = await _appointmentRepository.CreateAsync(appointment);

            if (result == null)
                return StatusCode(500, "Failed to create appointment. Please check the data or server logs.");

            var response = result.Adapt<AppointmentResponse>();
            return CreatedAtAction(nameof(GetAppointmentById), new { id = result.AppointmentId }, response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAppointment(int id, [FromBody] AppointmentRequest appointmentRequest)
        {
            if (appointmentRequest == null)
                return BadRequest("Appointment data is null.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var appointment = await _appointmentRepository.GetOneAsync(e => e.AppointmentId == id);
            if (appointment == null)
                return NotFound($"Appointment with ID {id} not found.");

            appointment.UserId = appointmentRequest.UserId;
            appointment.PetId = appointmentRequest.PetId;
            appointment.VetId = appointmentRequest.VetId;
            appointment.DateTime = appointmentRequest.DateTime ?? appointment.DateTime;
            appointment.Status = appointmentRequest.Status;

            var result = await _appointmentRepository.EditAsync(appointment);

            if (result == null)
                return StatusCode(500, "Failed to update appointment. Check server logs.");

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAppointment([FromRoute] int id)
        {
            var appointment = await _appointmentRepository.GetOneAsync(e => e.AppointmentId == id);
            if (appointment == null)
            {
                return NotFound($"Appointment with ID {id} not found.");
            }
            await _appointmentRepository.DeleteAsync(appointment);
            return NoContent();
        }
    }

}
