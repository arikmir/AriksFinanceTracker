#!/bin/bash

# Finance Tracker Database Restore Script
# Usage: ./restore.sh <backup_name>

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
API_DIR="$SCRIPT_DIR/AriksFinanceTracker.Api"
BACKUP_DIR="$API_DIR/backups"
DB_FILE="$API_DIR/ariks_finance.db"

if [ -z "$1" ]; then
    echo "Usage: $0 <backup_name>"
    echo ""
    echo "Available backups:"
    ls -la "$BACKUP_DIR"/*.db 2>/dev/null | while read -r line; do
        echo "  $line"
    done
    exit 1
fi

BACKUP_NAME="$1"
# Remove .db extension if provided
BACKUP_NAME="${BACKUP_NAME%.db}"
BACKUP_FILE="$BACKUP_DIR/${BACKUP_NAME}.db"

echo "Restoring from backup: $BACKUP_NAME"

# Check if backup exists
if [ ! -f "$BACKUP_FILE" ]; then
    echo "Error: Backup file not found at $BACKUP_FILE"
    echo ""
    echo "Available backups:"
    ls -la "$BACKUP_DIR"/*.db 2>/dev/null | while read -r line; do
        echo "  $line"
    done
    exit 1
fi

# Create backup of current database before restoring
if [ -f "$DB_FILE" ]; then
    TIMESTAMP=$(date +%Y%m%d_%H%M%S)
    CURRENT_BACKUP="$BACKUP_DIR/before_restore_$TIMESTAMP.db"
    echo "Creating backup of current database: before_restore_$TIMESTAMP.db"
    cp "$DB_FILE" "$CURRENT_BACKUP"
    
    # Create metadata for current backup
    FILE_SIZE=$(stat -f%z "$CURRENT_BACKUP" 2>/dev/null || stat -c%s "$CURRENT_BACKUP" 2>/dev/null)
    cat > "$BACKUP_DIR/before_restore_$TIMESTAMP.json" << EOF
{
  "fileName": "before_restore_$TIMESTAMP.db",
  "createdAt": "$(date -u +%Y-%m-%dT%H:%M:%S.%3NZ)",
  "description": "Backup created before restoring from $BACKUP_NAME",
  "fileSize": $FILE_SIZE
}
EOF
fi

# Restore from backup
cp "$BACKUP_FILE" "$DB_FILE"

if [ $? -eq 0 ]; then
    echo "Database restored successfully from: ${BACKUP_NAME}.db"
    echo "Previous database backed up as: before_restore_$TIMESTAMP.db"
    
    # Show backup metadata if available
    METADATA_FILE="$BACKUP_DIR/${BACKUP_NAME}.json"
    if [ -f "$METADATA_FILE" ]; then
        echo ""
        echo "Backup information:"
        cat "$METADATA_FILE"
    fi
else
    echo "Error: Failed to restore from backup"
    exit 1
fi