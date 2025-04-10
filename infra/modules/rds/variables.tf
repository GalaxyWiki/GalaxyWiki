variable "db_name" {
  description = "The name of the database"
  type        = string
  default     = "galaxy_db"
}

variable "db_username" {
  description = "The username for the database"
  type        = string
  default     = "galaxy_admin"
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