document.addEventListener('DOMContentLoaded', async () => {
    try {
        const response = await fetch('/OpenStreetMap/buildings');
        const buildings = await response.json();

        const buildingList = document.getElementById('building-list');
        buildings.forEach(building => {
            const buildingElement = document.createElement('div');
            buildingElement.innerHTML = `
                <p>Building ID: ${building.Id}</p>
                <p>Bounding Box:</p>
                <ul>
                    <li>Min Lat: ${building.BoundingBox.MinLat}</li>
                    <li>Max Lat: ${building.BoundingBox.MaxLat}</li>
                    <li>Min Lon: ${building.BoundingBox.MinLon}</li>
                    <li>Max Lon: ${building.BoundingBox.MaxLon}</li>
                </ul>
                <hr>
            `;
            buildingList.appendChild(buildingElement);
        });
    } catch (error) {
        console.error('Error fetching buildings:', error.message);
    }
});
