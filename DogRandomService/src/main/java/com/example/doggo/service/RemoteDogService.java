package com.example.doggo.service;

import com.example.doggo.model.Doggo;
import com.example.doggo.model.DoggoHistory;
import com.example.doggo.repository.DoggoHistoryRepository;
import com.example.doggo.repository.DoggoRepository;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;
import org.springframework.web.client.RestTemplate;

import java.util.List;

@Service
public class RemoteDogService {

    private final DoggoRepository dogRepo;
    private final DoggoHistoryRepository historyRepo;
    private final RestTemplate restTemplate = new RestTemplate();

    @Autowired
    public RemoteDogService(DoggoRepository dogRepo, DoggoHistoryRepository historyRepo) {
        this.dogRepo = dogRepo;
        this.historyRepo = historyRepo;
    }

    public Doggo fetchRandomDog() {
        Doggo dog = restTemplate.getForObject("https://random.dog/woof.json", Doggo.class);

        if (dog != null) {
            Doggo saved = dogRepo.save(dog);

            DoggoHistory history = DoggoHistory.builder()
                    .doggoId(saved.getId())
                    .url(saved.getUrl())
                    .sizeBytes(saved.getSizeBytes())
                    .build();

            historyRepo.save(history);
            return saved;
        }
        return null;
    }

    public List<Doggo> getAllDogs() {
        return dogRepo.findAll();
    }

    public List<DoggoHistory> getDogHistory() {
        return historyRepo.findAllByOrderByIdDesc();
    }

    public Doggo getDogById(Long id) {
        return dogRepo.findById(id).orElse(null);
    }
}
