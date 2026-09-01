CREATE USER "users_service_user" WITH PASSWORD 'users_service_user';
CREATE USER "events_service_user" WITH PASSWORD 'events_service_user';
CREATE USER "bookings_service_user" WITH PASSWORD 'bookings_service_user';

CREATE DATABASE "users" WITH OWNER "users_service_user";
CREATE DATABASE "events" WITH OWNER "events_service_user";
CREATE DATABASE "bookings" WITH OWNER "bookings_service_user";
