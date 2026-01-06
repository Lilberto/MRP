https://github.com/Lilberto/MRP

Unit Tests:
- dotnet test Tests/


If the 01-init.sql does not work:
CREATE TABLE users (
    id SERIAL PRIMARY KEY,
    username VARCHAR(50) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    salt VARCHAR(100) NOT NULL,
    token VARCHAR(255),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    token_created_At TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE media_entries (
    id SERIAL PRIMARY KEY,
    user_id INTEGER REFERENCES users(id) ON DELETE CASCADE,
    title VARCHAR(200) NOT NULL,
    description TEXT,
    media_type VARCHAR(20) CHECK (media_type IN ('movie', 'series', 'game')),
    release_year INTEGER,
    age_restriction VARCHAR(10) DEFAULT 'FSK0',
    avg_score DECIMAL(3,2) DEFAULT 0.0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE media_genres (
    media_id INTEGER REFERENCES media_entries(id) ON DELETE CASCADE,
    genre VARCHAR(50) NOT NULL,
    PRIMARY KEY (media_id, genre)
);

CREATE TABLE ratings (
    id SERIAL PRIMARY KEY,
    user_id INTEGER REFERENCES users(id) ON DELETE CASCADE,
    media_id INTEGER REFERENCES media_entries(id) ON DELETE CASCADE,
    stars INTEGER CHECK (stars BETWEEN 1 AND 5) NOT NULL,
    comment TEXT,
    comment_published BOOLEAN DEFAULT FALSE,
    likes_count INTEGER DEFAULT 0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(user_id, media_id)
);

CREATE TABLE rating_likes (
    rating_id INTEGER REFERENCES ratings(id) ON DELETE CASCADE,
    user_id INTEGER REFERENCES users(id) ON DELETE CASCADE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (rating_id, user_id)
);

CREATE TABLE favorites (
    user_id INTEGER REFERENCES users(id) ON DELETE CASCADE,
    media_id INTEGER REFERENCES media_entries(id) ON DELETE CASCADE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (user_id, media_id)
);



Select commands:

- All Media with Creator Info
SELECT m.*, u.username as creator 
FROM media_entries m 
JOIN users u ON m.user_id = u.id;

- Ratings with details
SELECT r.*, u.username, m.title 
FROM ratings r
JOIN users u ON r.user_id = u.id
JOIN media_entries m ON r.media_id = m.id;

- Media with genre
SELECT m.title, mg.genre
FROM media_entries m
INNER JOIN media_genres mg ON m.id = mg.media_id;

- Fav count & Users
SELECT u.username, COUNT(f.media_id) as favorite_count
FROM users u
LEFT JOIN favorites f ON u.id = f.user_id
GROUP BY u.id;

- Media/Genre/Rating Count
SELECT m.*, 
       ARRAY_AGG(mg.genre) as genres,
       COUNT(r.id) as rating_count
FROM media_entries m
LEFT JOIN media_genres mg ON m.id = mg.media_id
LEFT JOIN ratings r ON m.id = r.media_id
GROUP BY m.id;