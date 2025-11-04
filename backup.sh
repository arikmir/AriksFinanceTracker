#!/bin/bash

# Finance Tracker Database Backup Script
# Usage: ./backup.sh [backup_name]

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
API_DIR="$SCRIPT_DIR/AriksFinanceTracker.Api"
BACKUP_DIR="$API_DIR/backups"
DB_FILE="$API_DIR/ariks_finance.db"

# Create backup directory if it doesn't exist
mkdir -p "$BACKUP_DIR"

# Generate backup name
if [ -z "$1" ]; then
    TIMESTAMP=$(date +%Y%m%d_%H%M%S)
    BACKUP_NAME="manual_$TIMESTAMP"
else
    BACKUP_NAME="$1"
fi

BACKUP_FILE="$BACKUP_DIR/${BACKUP_NAME}.db"
METADATA_FILE="$BACKUP_DIR/${BACKUP_NAME}.json"

echo "Creating backup: $BACKUP_NAME"

# Check if database exists
if [ ! -f "$DB_FILE" ]; then
    echo "Error: Database file not found at $DB_FILE"
    exit 1
fi

# Copy database file
cp "$DB_FILE" "$BACKUP_FILE"

if [ $? -eq 0 ]; then
    # Get file size
    FILE_SIZE=$(stat -f%z "$BACKUP_FILE" 2>/dev/null || stat -c%s "$BACKUP_FILE" 2>/dev/null)
    
    # Create metadata
    cat > "$METADATA_FILE" << EOF
{
  "fileName": "${BACKUP_NAME}.db",
  "createdAt": "$(date -u +%Y-%m-%dT%H:%M:%S.%3NZ)",
  "description": "Manual backup created at $(date)",
  "fileSize": $FILE_SIZE
}
EOF
    
    echo "Backup created successfully: ${BACKUP_NAME}.db"
    echo "Backup location: $BACKUP_FILE"
    echo "File size: $FILE_SIZE bytes"
else
    echo "Error: Failed to create backup"
    exit 1
fi

# List recent backups
echo ""
echo "Recent backups:"
ls -la "$BACKUP_DIR"/*.db 2>/dev/null | tail -5 | while read -r line; do
    echo "  $line"
done