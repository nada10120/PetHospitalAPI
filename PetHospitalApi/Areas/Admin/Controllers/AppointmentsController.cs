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
            return Ok(appointments.Adapt<AppointmentResponse>());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAppointmentById([FromRoute] int id)
        {
            var appointment = await _appointmentRepository.GetAsync(e => e.AppointmentId == id);
            if (appointment == null)
            {
                return NotFound($"Appointment with ID {id} not found.");
            }
            return Ok(appointment.Adapt<AppointmentResponse>());
        }
        [HttpPost]
        public async Task<IActionResult> CreateAppointment([FromBody] AppointmentRequest appointmentrequest)
        {
            if (appointmentrequest == null)
            {
                return BadRequest("Appointment data is null.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var appointment = new Appointment
            {
                UserId = appointmentrequest.UserId,
                PetId = appointmentrequest.PetId,
                VetId = appointmentrequest.VetId,
                //DateTime = appointmentrequest.DateTime,
                Status = appointmentrequest.Status
            };
            await _appointmentRepository.CreateAsync(appointment);
            return CreatedAtAction(nameof(GetAppointmentById), new { id = appointment.AppointmentId }, appointment);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAppointment([FromRoute] int id, [FromBody] AppointmentRequest appointmentrequest)
        {
            if (appointmentrequest == null)
            {
                return BadRequest("Appointment data is null.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var appointment = await _appointmentRepository.GetOneAsync(e => e.AppointmentId == id);
            if (appointment == null)
            {
                return NotFound($"Appointment with ID {id} not found.");
            }
            appointment.UserId = appointmentrequest.UserId;
            appointment.PetId = appointmentrequest.PetId;
            appointment.VetId = appointmentrequest.VetId;
            appointment.Status = appointmentrequest.Status;
            await _appointmentRepository.EditAsync(appointment);
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
