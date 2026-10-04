package main

import (
	"encoding/csv"
	"encoding/json"
	"fmt"
	"os"
	"path/filepath"
	"strings"

	gmaps "github.com/GMaps-Lead-Finder/google-maps-scraper/go"
)

func main() {
	if len(os.Args) < 2 {
		printHelp()
		os.Exit(1)
	}
	cmd := os.Args[1]
	switch cmd {
	case "-h", "--help", "help":
		printHelp()
	case "--version", "version":
		fmt.Println("0.1.2")
	case "me":
		client, err := gmaps.NewClient(nil)
		must(err)
		me, err := client.Me()
		must(err)
		enc := json.NewEncoder(os.Stdout)
		enc.SetIndent("", "  ")
		_ = enc.Encode(me)
	case "scrape":
		if len(os.Args) < 3 || strings.HasPrefix(os.Args[2], "-") {
			fmt.Fprintln(os.Stderr, `error: scrape requires a keyword, e.g. scrape "dentists in Austin TX"`)
			os.Exit(1)
		}
		keyword := os.Args[2]
		var outPath string
		opts := &gmaps.ScrapeOptions{}
		for i := 3; i < len(os.Args); i++ {
			switch os.Args[i] {
			case "--out":
				i++
				if i >= len(os.Args) {
					fmt.Fprintln(os.Stderr, "error: --out requires a path")
					os.Exit(1)
				}
				outPath = os.Args[i]
			case "--poll-interval-ms":
				i++
				fmt.Sscanf(os.Args[i], "%d", &opts.PollIntervalMs)
			case "--timeout-ms":
				i++
				fmt.Sscanf(os.Args[i], "%d", &opts.TimeoutMs)
			default:
				fmt.Fprintf(os.Stderr, "error: unknown argument %s\n", os.Args[i])
				os.Exit(1)
			}
		}
		client, err := gmaps.NewClient(nil)
		must(err)
		rows, err := client.Scrape(keyword, opts)
		must(err)
		must(writeOutput(rows, outPath))
	default:
		fmt.Fprintf(os.Stderr, "error: unknown command %s\n", cmd)
		printHelp()
		os.Exit(1)
	}
}

func printHelp() {
	fmt.Print(`gmaps-scraper (Go) 0.1.2

Usage:
  go run ./cli me
  go run ./cli scrape "<keyword>" [--out leads.json|leads.csv]

Env:
  GMF_API_KEY     required
  GMF_BASE_URL    optional
`)
}

func writeOutput(rows []gmaps.PlaceRow, outPath string) error {
	if outPath == "" {
		enc := json.NewEncoder(os.Stdout)
		enc.SetIndent("", "  ")
		return enc.Encode(rows)
	}
	dir := filepath.Dir(outPath)
	if dir != "." && dir != "" {
		if err := os.MkdirAll(dir, 0o755); err != nil {
			return err
		}
	}
	if strings.HasSuffix(strings.ToLower(outPath), ".csv") {
		return writeCSV(outPath, rows)
	}
	b, err := json.MarshalIndent(rows, "", "  ")
	if err != nil {
		return err
	}
	if err := os.WriteFile(outPath, append(b, '\n'), 0o644); err != nil {
		return err
	}
	fmt.Fprintf(os.Stderr, "Wrote %d rows to %s\n", len(rows), outPath)
	return nil
}

func writeCSV(path string, rows []gmaps.PlaceRow) error {
	var fields []string
	seen := map[string]struct{}{}
	for _, row := range rows {
		for k := range row {
			if _, ok := seen[k]; !ok {
				seen[k] = struct{}{}
				fields = append(fields, k)
			}
		}
	}
	f, err := os.Create(path)
	if err != nil {
		return err
	}
	defer f.Close()
	w := csv.NewWriter(f)
	if len(fields) == 0 {
		fields = []string{"Name"}
	}
	if err := w.Write(fields); err != nil {
		return err
	}
	for _, row := range rows {
		rec := make([]string, len(fields))
		for i, k := range fields {
			rec[i] = row[k]
		}
		if err := w.Write(rec); err != nil {
			return err
		}
	}
	w.Flush()
	fmt.Fprintf(os.Stderr, "Wrote %d rows to %s\n", len(rows), path)
	return w.Error()
}

func must(err error) {
	if err != nil {
		fmt.Fprintln(os.Stderr, "error:", err)
		os.Exit(1)
	}
}
