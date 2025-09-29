// mongo-init.js - Script de inicialización de MongoDB para SoftFocus
// Este script se ejecuta automáticamente al crear el container por primera vez

// Cambiar a la base de datos de la aplicación
db = db.getSiblingDB('softfocus_db');

// Crear usuario para la aplicación
db.createUser({
    user: 'softfocus_user',
    pwd: process.env.MONGO_USER_PASSWORD,
    roles: [
        {
            role: 'readWrite',
            db: 'softfocus_db'
        }
    ]
});

// Crear índices para optimizar consultas según tu arquitectura
db.users.createIndex({ email: 1 }, { unique: true });
db.users.createIndex({ role: 1 });

print('✅ MongoDB initialization completed for SoftFocus');
print('📊 Database: softfocus_db');
print('👤 Application user: softfocus_user');