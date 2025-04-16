variable "db_name" {
  description = "The name of the database"
  type        = string
  default     = "galaxy"
}

variable "db_username" {
  description = "The username for the database"
  type        = string
  default     = "galaxy"
}

variable "db_password" {
  description = "The password for the database"
  type        = string
  sensitive   = true
}

variable "db_port" {
  description = "The port for the database"
  type        = number
  default     = 5432
}

variable "db_host" {
  description = "The host for the database"
  type        = string
  default     = "localhost"
} 

// locals {
//   envs = { for tuple in regexall("(.*)=(.*)", file(".env")) : tuple[0] => sensitive(tuple[1]) }
// }