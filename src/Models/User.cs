/**
	Copyright 2021 Hirad Asadi (API in a Box)

	Licensed under the Apache License, Version 2.0 (the "License");
	you may not use this file except in compliance with the License.
	You may obtain a copy of the License at

		http://www.apache.org/licenses/LICENSE-2.0

	Unless required by applicable law or agreed to in writing, software
	distributed under the License is distributed on an "AS IS" BASIS,
	WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
	See the License for the specific language governing permissions and
	limitations under the License.
*/

using ApiInABox.Models.Auth;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ApiInABox.Models
{
    public class User : AbstractDbBase
    {
        public User()
        {
            Roles = new HashSet<Role>();
        }

        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string ActivationEmail { get; set; }
        [JsonIgnore]
        public bool Activated { get; set; }
        [JsonIgnore]
        public string TemporarySecret { get; set; }
        public ICollection<Role> Roles { get; set; }
    }
}
