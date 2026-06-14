const express = require("express")
const cors = require("cors")
const app = express()
const pool = require("./db")
const PORT = process.env.PORT;

// middleware
app.use(cors())
app.use(express.json())


// routes
app.get("/videos3/songs", async (req, res) => {
    try {
        const allData = await pool.query("SELECT DISTINCT songname, creator FROM videos3 ORDER BY songname");
        res.json(allData.rows);
    } catch (err) {
        console.error(err.message);
    }
});


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