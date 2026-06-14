const express = require("express")
const cors = require("cors")
const app = express()
const pool = require("./db")
const PORT = process.env.PORT;

// middleware
app.use(cors())
app.use(express.json())


// routes
// syncit2.com ##########################################################################################################
app.get("/videos3/songs", async (req, res) => {
    try {
        const allData = await pool.query("SELECT DISTINCT songname, creator FROM videos3 ORDER BY songname");
        res.json(allData.rows);
    } catch (err) {
        console.error(err.message);
    }
});

app.get("/videos3/:songname", async (req, res) => {
    try {
        const { songname } = req.params;
        const songnames = await pool.query("SELECT * FROM videos3 WHERE songname = $1 ORDER BY CASE row_position WHEN 'top' THEN 0 WHEN 'bot' THEN 1 ELSE 2 END, column_position", [songname]);
        res.json(songnames.rows);
    } catch (err) {
        console.error(err.message);
    }
});

app.get("/songsRequested/songs", async (req, res) => {
    try {
        const allData = await pool.query("SELECT DISTINCT randomsongid FROM songsRequested");
        res.json(allData.rows);
    } catch (err) {
        console.error(err.message);
    }
});

app.post("/songsRequested/newSong", async (req, res) => {
  const rows = req.body;

  try {
    const query = `
      INSERT INTO songsrequested (randomsongid, youtube_id, start_time, row_position, column_position, creator)
      VALUES ($1, $2, $3, $4, $5, $6)
    `;

    for (const row of rows) {
      await pool.query(query, [
        row.randomsongid,
        row.youtube_id,
        row.start_time,
        row.row_position,
        row.column_position,
        row.creator
      ]);
    }

    res.status(201).json({ message: "Rows inserted successfully" });
  } catch (err) {
    console.error(err);
    res.status(500).json({ error: "Database insert failed" });
  }
});


// kartpop.com ##########################################################################################################
app.get("/kartpop", async (req, res) => {
    try {
        const allData = await pool.query("SELECT * FROM komlatlong");
        res.json(allData.rows);
    } catch (err) {
        console.error(err.message);
    }
});

app.listen(PORT, () => {
  console.log(`Server running on port ${PORT}`);
});