variable "db_host" {
  description = "The host of the database"
  type        = string
}

variable "db_port" {
  description = "The port of the database"
  type        = number
}

variable "db_name" {
  description = "The name of the database"
  type        = string
}

variable "db_username" {
  description = "The username for the database"
  type        = string
}

variable "db_password" {
  description = "The password for the database"
  type        = string
  sensitive   = true
}